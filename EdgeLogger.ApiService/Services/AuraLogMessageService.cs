using System.Text.Json.Serialization;
using System.Threading.Channels;
using LiteDB;
using NATS.Net;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EdgeLogger.ApiService.Services;

public class AuraLogMessageService(NatsClient natsClient, ILogger<AuraLogMessageService> logger) : BackgroundService
{
    private static string DatabasePath 
    {
        get
        {
            var basePath = Environment.GetEnvironmentVariable("EDGELOGGER_DATA_DIR")
                           ?? BuildAndEnsureDirectoryPath();

            var dbPath = Path.Combine(basePath, "edgeLogger.db");
            return dbPath;

            static string BuildAndEnsureDirectoryPath()
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EdgeLogger");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }
    }

    private readonly Channel<Tuple<string, AuraLogMessage>> logChannel = Channel.CreateUnbounded<Tuple<string, AuraLogMessage>>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumeTask = Task.Run(() => ConsumeNats(stoppingToken), stoppingToken);
        var processTask = Task.Run(() => ConsumeChannel(stoppingToken), stoppingToken);

        await Task.WhenAll(consumeTask, processTask);
    }

    private async Task ConsumeNats(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in natsClient.SubscribeAsync<string>("aura2.logs.>", cancellationToken: stoppingToken))
            {
                try
                {
                    var deviceName = message.Subject.Split(".")[^1];
                    var logMessage = JsonSerializer.Deserialize<AuraLogMessage>(message.Data!);
                    logger.LogDebug("Received Aura log message from {DeviceName}: {LogMessage}", deviceName, logMessage);

                    if (logMessage is not null)
                        await logChannel.Writer.WriteAsync(Tuple.Create(deviceName, logMessage), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Aura log message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is stopping.
        }
    }

    private async Task ConsumeChannel(CancellationToken stoppingToken)
    {
        try
        {
            while (await logChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                var batch = new List<Tuple<string, AuraLogMessage>>();
                while (batch.Count < 50 && logChannel.Reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Any())
                {
                    var storeBatch = batch.Select(t => new AuraLogStore(
                                                      Timestamp: DateTimeOffset.FromUnixTimeSeconds(t.Item2.Timestamp).UtcDateTime,
                                                      Message: t.Item2.Message,
                                                      DeviceName: t.Item1))
                                          .ToList();
                
                    using var db = new LiteDatabase(DatabasePath);
                    var collection = db.GetCollection<AuraLogStore>("AuraLogs");
                    collection.EnsureIndex(x => x.DeviceName);

                    collection.InsertBulk(storeBatch);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is stopping.
        }
    }
}

public record AuraLogMessage(
    [property: JsonPropertyName("timestamp")] long Timestamp,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("message_length")] int MessageLen
);

public record AuraLogStore(
    DateTime Timestamp,
    string Message,
    string DeviceName
);

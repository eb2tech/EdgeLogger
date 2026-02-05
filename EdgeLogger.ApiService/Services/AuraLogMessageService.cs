using System.Text.Json.Serialization;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

                    WriteLogMessage(deviceName, logMessage);
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

    private void WriteLogMessage(string deviceName, AuraLogMessage? logMessage)
    {
        if (logMessage is null) return;

        var logStore = new AuraLogStore(
            Timestamp: DateTimeOffset.FromUnixTimeSeconds(logMessage.Timestamp).UtcDateTime,
            Message: logMessage.Message,
            DeviceName: deviceName
        );

        using var db = new LiteDatabase(DatabasePath);
        var collection = db.GetCollection<AuraLogStore>("AuraLogs");
        collection.EnsureIndex(x => x.DeviceName);

        collection.Insert(logStore);
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

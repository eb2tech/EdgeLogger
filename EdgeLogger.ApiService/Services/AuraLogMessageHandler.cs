using NATS.Net;

namespace EdgeLogger.ApiService.Services;

public class AuraLogMessageHandler(NatsClient natsClient, ILogger<AuraLogMessageHandler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in natsClient.SubscribeAsync<string>("aura2.logs.>", cancellationToken: stoppingToken))
        {
            try
            {
                var deviceName = message.Subject.Split(".")[^1];
                var logMessage = message.Data;
                logger.LogInformation("Received Aura log message from {DeviceName}: {LogMessage}", deviceName, logMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Aura log message");
            }
        }
    }
}
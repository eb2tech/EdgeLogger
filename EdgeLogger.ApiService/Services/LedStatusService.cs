namespace EdgeLogger.ApiService.Services;

internal class LedStatusService(INetworkStatus networkStatus, ILogger<LedStatusService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!PiIntrinsics.IsRunningOnPi())
        {
            logger.LogInformation("LedStatusService is only supported on Raspberry Pi devices. Exiting service.");
            return;
        }

        networkStatus.StatusChanged += OnNetworkStatusChanged;

        var currentStatus = networkStatus.Status;
        OnNetworkStatusChanged(this, currentStatus);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static void OnNetworkStatusChanged(object? sender, NetworkState e)
    {
        switch (e)
        {
            case NetworkState.ConnectedGlobal:
            case NetworkState.ConnectedLocal:
            case NetworkState.ConnectedSite:
                PiIntrinsics.SetLedStateNormal();
                break;
            case NetworkState.Disconnected:
                PiIntrinsics.SetLedStateFastBlink();
                break;
            case NetworkState.Disconnecting:
            case NetworkState.Connecting:
                PiIntrinsics.SetLedStateSlowBlink();
                break;
            default:
                PiIntrinsics.SetLedStatePulse();
                break;
        }
    }
}
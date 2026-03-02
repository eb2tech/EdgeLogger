using Tmds.DBus.Protocol;

namespace EdgeLogger.ApiService.Services;

internal class NetworkStateMonitorService(ISetNetworkStatus networkStatus, ILogger<NetworkStateMonitorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!PiIntrinsics.IsRunningOnPi())
        {
            logger.LogInformation("NetworkStateMonitorService is only supported on Raspberry Pi devices. Exiting service.");
            return;
        }

        using var connection = new DBusConnection(DBusAddress.System!);
        await connection.ConnectAsync();
        var nm = new NetworkManager(connection, "org.freedesktop.NetworkManager", "/org/freedesktop/NetworkManager");

        await nm.WatchStateChangedAsync((ex, state) =>
        {
            var networkState = (NetworkState)state;
            networkStatus.SetStatus(networkState);
            logger.LogInformation(ex, "Network state changed: {Status}", networkState);
        });

        var networkState = await nm.GetState();
        networkStatus.SetStatus(networkState);

        logger.LogInformation("Initial network state: {Status}", networkState);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
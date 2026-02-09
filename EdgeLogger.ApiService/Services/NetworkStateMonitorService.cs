using Tmds.DBus.Protocol;

namespace EdgeLogger.ApiService.Services;

internal class NetworkStateMonitorService(ILogger<NetworkStateMonitorService> logger) : BackgroundService, INetworkStatus
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!PiIntrinsics.IsRunningOnPi())
        {
            logger.LogInformation("NetworkStateMonitorService is only supported on Raspberry Pi devices. Exiting service.");
            return;
        }

        using var connection = new DBusConnection(DBusAddress.System!);
        var nm = new NetworkManager(connection, "org.freedesktop.NetworkManager", "/org/freedesktop/NetworkManager");

        await nm.WatchStateChangedAsync((ex, state) =>
        {
            Status = (NetworkState)state;
            IsConnected = MapConnectedState(Status);

            logger.LogInformation(ex, "Network state changed: {Status}", Status);

            StatusChanged?.Invoke(this, Status);
        });

        Status = await nm.GetState();
        IsConnected = MapConnectedState(Status);

        logger.LogInformation("Initial network state: {Status}", Status);

        return;

        static bool MapConnectedState(NetworkState state)
        {
            return state switch
            {
                NetworkState.ConnectedLocal => true,
                NetworkState.ConnectedSite => true,
                NetworkState.ConnectedGlobal => true,
                _ => false
            };
        }
    }

    public NetworkState Status { get; private set; } = NetworkState.Unknown;

    public bool IsConnected { get; private set; }

    public event EventHandler<NetworkState>? StatusChanged;
}
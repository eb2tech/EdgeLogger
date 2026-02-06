namespace EdgeLogger.ApiService.Services;

internal interface INetworkStatus
{
    NetworkState Status { get; }
    bool IsConnected { get; }
    event EventHandler<NetworkState> StatusChanged;
}
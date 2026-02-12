namespace EdgeLogger.ApiService.Services;

internal interface INetworkStatus
{
    NetworkState Status { get; }
    bool IsConnected { get; }
    event EventHandler<NetworkState> StatusChanged;
}

internal interface ISetNetworkStatus
{
    void SetStatus(NetworkState status);
}

internal class NetworkStatus : INetworkStatus, ISetNetworkStatus
{
    public NetworkState Status { get; private set; } = NetworkState.Unknown;
    public bool IsConnected { get; private set; }
    public event EventHandler<NetworkState>? StatusChanged;
    public void SetStatus(NetworkState status)
    {
        Status = status;
        IsConnected = status switch
        {
            NetworkState.ConnectedLocal => true,
            NetworkState.ConnectedSite => true,
            NetworkState.ConnectedGlobal => true,
            _ => false
        };
        StatusChanged?.Invoke(this, Status);
    }
}
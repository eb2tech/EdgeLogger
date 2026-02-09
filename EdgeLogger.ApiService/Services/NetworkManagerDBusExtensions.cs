namespace EdgeLogger.ApiService.Services;

internal static class NetworkManagerDBusExtensions
{
    extension(NetworkManager nm)
    {
        internal async Task<NetworkState> GetState()
        {
            var state = await nm.StateAsync();
            return (NetworkState)state;
        }
    }
}
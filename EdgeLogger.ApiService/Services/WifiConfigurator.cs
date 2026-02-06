namespace EdgeLogger.ApiService.Services;

internal class WifiConfigurator : IWifiConfigurator
{
    public Task<bool> ConfigureAndConnectAsync(string ssid, string password, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
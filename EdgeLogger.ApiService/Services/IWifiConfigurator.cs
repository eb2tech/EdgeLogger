namespace EdgeLogger.ApiService.Services;

internal interface IWifiConfigurator
{
    Task<bool> ConfigureAndConnectAsync(string ssid, string password, CancellationToken cancellationToken);
}
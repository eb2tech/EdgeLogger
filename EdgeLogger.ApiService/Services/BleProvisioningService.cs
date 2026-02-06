using System.Text;
using Linux.Bluetooth;
using Linux.Bluetooth.GattServer;

namespace EdgeLogger.ApiService.Services;

internal class BleProvisioningService(
    INetworkStatus networkStatus,
    IWifiConfigurator wifiConfigurator,
    ILogger<BleProvisioningService> logger) : BackgroundService
{
    private const string ServiceUuid = "12345678-1234-5678-1234-56789abcdef0";
    private const string SsidCharUuid = "12345678-1234-5678-1234-56789abcdef1";
    private const string PasswordCharUuid = "12345678-1234-5678-1234-56789abcdef2";
    private const string CommandCharUuid = "12345678-1234-5678-1234-56789abcdef3";

    private GattServer? bleServer;
    private bool isProvisioning;
    private string pendingSsid = string.Empty;
    private string pendingPassword = string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!PiIntrinsics.IsRunningOnPi())
        {
            logger.LogWarning("BLE provisioning service is only supported on Raspberry Pi devices. Exiting service.");
            return;
        }

        networkStatus.StatusChanged += OnNetworkStatusChanged;

        if (!networkStatus.IsConnected)
            await BeginBleProvisioningAsync();
    }

    private void OnNetworkStatusChanged(object? sender, NetworkState e)
    {
        switch (e)
        {
            case NetworkState.Disconnected or NetworkState.Asleep or NetworkState.Unknown:
                Task.Factory.StartNew(async () => await BeginBleProvisioningAsync());
                break;
            case NetworkState.ConnectedLocal or NetworkState.ConnectedSite or NetworkState.ConnectedGlobal:
                Task.Factory.StartNew(async () => await EndBleProvisioningAsync());
                break;
        }
    }

    private async Task BeginBleProvisioningAsync()
    {
        try
        {
            logger.LogInformation("Starting BLE provisioning service...");

            if (isProvisioning)
            {
                logger.LogWarning("Already provisioning");
                return;
            }

            var adapter = await GetDefaultAdapterAsync();
            if (adapter is null)
            {
                logger.LogWarning("No BLE Adapters found.");
                return;
            }

            // Power on the adapter
            await adapter.SetPoweredAsync(true);

            bleServer = new GattServer(adapter);
            await bleServer.InitializeAsync(); // Connect to DBus

            // Start advertising
            var advertisementOptions = new LEAdvertisement1Properties
                                       {
                                           Type = "peripheral",
                                           LocalName = "picollect-Setup",
                                           Discoverable = true,
                                           IncludeTxPower = true
                                       };
            var advertisement = bleServer.CreateAdvertisement(advertisementOptions);
            await bleServer.RegisterAdvertisement(advertisement);

            bleServer.CreateGattApplication();

            var service = bleServer.CreateService(new GattService1Properties
                                                   {
                                                       UUID = ServiceUuid,
                                                       Primary = true
                                                   });

            var ssidCharacteristic = service.AddCharacteristic(new GattCharacteristic1Properties
                                                               {
                                                                   UUID = SsidCharUuid,
                                                                   Flags = [DescriptorFlags.Write]
                                                               });
            ssidCharacteristic.WriteValueEvent += (_, arg) =>
            {
                var ssid = Encoding.UTF8.GetString(arg.Value);
                logger.LogInformation("Received SSID: {Ssid}", ssid);
                pendingSsid = ssid;
                return Task.CompletedTask;
            };

            var passwordCharacteristic = service.AddCharacteristic(new GattCharacteristic1Properties
                                                                   {
                                                                       UUID = PasswordCharUuid,
                                                                       Flags = [CharacteristicFlags.Write]
                                                                   });
            passwordCharacteristic.WriteValueEvent += (_, arg) =>
            {
                var password = Encoding.UTF8.GetString(arg.Value);
                logger.LogInformation("Received Password: {Password}", password);
                pendingPassword = password;
                return Task.CompletedTask;
            };

            var commandCharacteristic = service.AddCharacteristic(new GattCharacteristic1Properties
                                                                  {
                                                                      UUID = CommandCharUuid,
                                                                      Flags = [CharacteristicFlags.Write]
                                                                  });
            commandCharacteristic.WriteValueEvent += async (_, arg) =>
            {
                var command = Encoding.UTF8.GetString(arg.Value);
                logger.LogInformation("Received Command: {Command}", command);
                await ProcessCommand(command);
            };

            isProvisioning = true;

            logger.LogInformation("BLE provisioning started.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BLE provisioning error.");
        }

        return;

        static async Task<Adapter?> GetDefaultAdapterAsync()
        {
            var adapters = await BlueZManager.GetAdaptersAsync();
            return adapters.Count == 0 ? null : adapters[0];
        }
    }

    private async Task EndBleProvisioningAsync()
    {
        if (!isProvisioning)
        {
            logger.LogWarning("Not currently provisioning");
            return;
        }

        if (bleServer is not null)
        {
            await bleServer.UnregisterAdvertisement();
            await bleServer.UnregisterGattApplication();
            bleServer.Dispose();
            bleServer = null;

            isProvisioning = false;
            logger.LogInformation("BLE provisioning stopped.");
        }
    }

    private async Task ProcessCommand(string s)
    {
        logger.LogInformation("Processing command: {Command}", s);

        if (string.IsNullOrWhiteSpace(pendingSsid))
        {
            logger.LogWarning("SSID not set");
            return;
        }

        if (string.IsNullOrWhiteSpace(pendingPassword))
        {
            logger.LogWarning("Password not set");
            return;
        }

        if (s.Equals("provision", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Attempting to provision for WiFi network {Ssid}", pendingSsid);
            var success = await wifiConfigurator.ConfigureAndConnectAsync(pendingSsid, pendingPassword, CancellationToken.None);
            if (success)
            {
                logger.LogInformation("Successfully provisioned for WiFi network {Ssid}", pendingSsid);
            }
            else
            {
                logger.LogWarning("Failed to provision for WiFi network {Ssid}", pendingSsid);
            }
        }
    }
}


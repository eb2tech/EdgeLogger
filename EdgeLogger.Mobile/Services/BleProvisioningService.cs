using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Text;

namespace EdgeLogger.Mobile.Services;

public class BleProvisioningService : IBleProvisioningService
{
    private const string PiDeviceName = "picollect-Setup";
    private const string ServiceUuid = "12345678-1234-5678-1234-56789abcdef0";
    private const string SsidCharUuid = "12345678-1234-5678-1234-56789abcdef1";
    private const string PasswordCharUuid = "12345678-1234-5678-1234-56789abcdef2";
    private const string CommandCharUuid = "12345678-1234-5678-1234-56789abcdef3";
    private const int MaxRetries = 3;
    private const int ScanTimeout = 10000; // 10 seconds

    private readonly IBluetoothLE _bluetoothLE;
    private readonly IAdapter _adapter;
    private IDevice? _connectedDevice;
    private IService? _provisioningService;

    public event EventHandler<bool>? ConnectionStateChanged;

    public bool IsBluetoothAvailable => _bluetoothLE.IsAvailable && _bluetoothLE.IsOn;
    public bool IsScanning => _adapter.IsScanning;
    public bool IsConnected => _connectedDevice?.State == DeviceState.Connected;

    public BleProvisioningService()
    {
        _bluetoothLE = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;
        
        _adapter.DeviceConnectionLost += OnDeviceConnectionLost;
    }

    public async Task<bool> ScanAndConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsBluetoothAvailable)
        {
            return false;
        }

        try
        {
            // If already connected, disconnect first
            if (IsConnected)
            {
                await DisconnectAsync();
            }

            // Scan for devices
            var scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            scanCts.CancelAfter(ScanTimeout);

            var devices = new List<IDevice>();
            _adapter.DeviceDiscovered += (s, e) =>
            {
                if (e.Device.Name == PiDeviceName)
                {
                    devices.Add(e.Device);
                }
            };

            await _adapter.StartScanningForDevicesAsync(cancellationToken: scanCts.Token);

            if (devices.Count == 0)
            {
                return false;
            }

            // Connect to the first Pi found
            var piDevice = devices[0];
            await _adapter.ConnectToDeviceAsync(piDevice, cancellationToken: cancellationToken);
            _connectedDevice = piDevice;

            // Discover services
            _provisioningService = await _connectedDevice.GetServiceAsync(Guid.Parse(ServiceUuid));
            if (_provisioningService == null)
            {
                await DisconnectAsync();
                return false;
            }

            ConnectionStateChanged?.Invoke(this, true);
            return true;
        }
        catch (Exception)
        {
            await DisconnectAsync();
            return false;
        }
    }

    public async Task<bool> ProvisionWiFiAsync(string ssid, string password, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _provisioningService == null)
        {
            return false;
        }

        try
        {
            // Get characteristics
            var ssidChar = await _provisioningService.GetCharacteristicAsync(Guid.Parse(SsidCharUuid));
            var passwordChar = await _provisioningService.GetCharacteristicAsync(Guid.Parse(PasswordCharUuid));
            var commandChar = await _provisioningService.GetCharacteristicAsync(Guid.Parse(CommandCharUuid));

            if (ssidChar == null || passwordChar == null || commandChar == null)
            {
                return false;
            }

            // Write SSID with retry
            if (!await WriteCharacteristicWithRetryAsync(ssidChar, Encoding.UTF8.GetBytes(ssid), cancellationToken))
            {
                return false;
            }

            // Write Password with retry
            if (!await WriteCharacteristicWithRetryAsync(passwordChar, Encoding.UTF8.GetBytes(password), cancellationToken))
            {
                return false;
            }

            // Write provision command with retry
            if (!await WriteCharacteristicWithRetryAsync(commandChar, Encoding.UTF8.GetBytes("provision"), cancellationToken))
            {
                return false;
            }

            // Wait a moment for the Pi to process the command
            await Task.Delay(1000, cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connectedDevice != null && IsConnected)
        {
            await _adapter.DisconnectDeviceAsync(_connectedDevice);
            ConnectionStateChanged?.Invoke(this, false);
        }

        _connectedDevice = null;
        _provisioningService = null;
    }

    private async Task<bool> WriteCharacteristicWithRetryAsync(
        ICharacteristic characteristic, 
        byte[] data, 
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                await characteristic.WriteAsync(data);
                return true;
            }
            catch (Exception)
            {
                if (attempt == MaxRetries - 1)
                {
                    return false;
                }
                await Task.Delay(500, cancellationToken); // Wait before retry
            }
        }

        return false;
    }

    private void OnDeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        if (e.Device.Id == _connectedDevice?.Id)
        {
            _connectedDevice = null;
            _provisioningService = null;
            ConnectionStateChanged?.Invoke(this, false);
        }
    }
}

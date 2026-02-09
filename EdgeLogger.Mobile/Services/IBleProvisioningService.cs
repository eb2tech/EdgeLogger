namespace EdgeLogger.Mobile.Services;

public interface IBleProvisioningService
{
    /// <summary>
    /// Gets whether Bluetooth is available and enabled on this device.
    /// </summary>
    bool IsBluetoothAvailable { get; }

    /// <summary>
    /// Gets whether the service is currently scanning for devices.
    /// </summary>
    bool IsScanning { get; }

    /// <summary>
    /// Gets whether the service is connected to a Pi device.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Scans for the EdgeLogger Pi device advertising as "picollect-Setup".
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if Pi was found and connected, false otherwise.</returns>
    Task<bool> ScanAndConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends WiFi credentials to the connected Pi.
    /// </summary>
    /// <param name="ssid">WiFi network SSID.</param>
    /// <param name="password">WiFi password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if provisioning succeeded, false otherwise.</returns>
    Task<bool> ProvisionWiFiAsync(string ssid, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the currently connected device.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Event raised when connection state changes.
    /// </summary>
    event EventHandler<bool> ConnectionStateChanged;
}

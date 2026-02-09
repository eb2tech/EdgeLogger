using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdgeLogger.Mobile.Services;

namespace EdgeLogger.Mobile.PageModels;

public partial class WiFiProvisioningPageModel : ObservableObject
{
    private readonly IBleProvisioningService _bleService;

    [ObservableProperty]
    private string _ssid = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isProvisioning;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _connectionIndicatorColor = "Gray";

    public WiFiProvisioningPageModel(IBleProvisioningService bleService)
    {
        _bleService = bleService;
        _bleService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    partial void OnSsidChanged(string value) => ProvisionWiFiCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => ProvisionWiFiCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanScanForPi))]
    private async Task ScanForPi()
    {
        if (!_bleService.IsBluetoothAvailable)
        {
            await App.Current!.MainPage!.DisplayAlert(
                "Bluetooth Unavailable",
                "Please enable Bluetooth to scan for devices.",
                "OK");
            return;
        }

        IsScanning = true;
        StatusMessage = "Scanning for Pi...";
        ConnectionIndicatorColor = "Orange";

        try
        {
            var success = await _bleService.ScanAndConnectAsync();

            if (success)
            {
                StatusMessage = "Connected to Pi";
                ConnectionIndicatorColor = "Green";
            }
            else
            {
                StatusMessage = "Pi not found";
                ConnectionIndicatorColor = "Red";
                await App.Current!.MainPage!.DisplayAlert(
                    "Device Not Found",
                    "Could not find the EdgeLogger Pi. Make sure it's powered on and in provisioning mode.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
            ConnectionIndicatorColor = "Red";
            await App.Current!.MainPage!.DisplayAlert("Scan Error", ex.Message, "OK");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private bool CanScanForPi() => !IsScanning && !IsProvisioning;

    [RelayCommand(CanExecute = nameof(CanProvisionWiFi))]
    private async Task ProvisionWiFi()
    {
        IsProvisioning = true;
        StatusMessage = "Provisioning WiFi...";

        try
        {
            var success = await _bleService.ProvisionWiFiAsync(Ssid, Password);

            if (success)
            {
                StatusMessage = "Provisioning successful!";
                await App.Current!.MainPage!.DisplayAlert(
                    "Success",
                    "WiFi provisioning completed. The Pi will connect to the network.",
                    "OK");

                // Clear fields
                Ssid = string.Empty;
                Password = string.Empty;

                // Wait for Pi to disconnect (it stops advertising when connected)
                await Task.Delay(2000);

                // Disconnect from Pi
                await _bleService.DisconnectAsync();
            }
            else
            {
                StatusMessage = "Provisioning failed";
                await App.Current!.MainPage!.DisplayAlert(
                    "Failed",
                    "WiFi provisioning failed. Please try again.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await App.Current!.MainPage!.DisplayAlert("Provisioning Error", ex.Message, "OK");
        }
        finally
        {
            IsProvisioning = false;
        }
    }

    private bool CanProvisionWiFi() =>
        IsConnected &&
        !IsProvisioning &&
        !string.IsNullOrWhiteSpace(Ssid) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private void OnConnectionStateChanged(object? sender, bool isConnected)
    {
        IsConnected = isConnected;

        if (isConnected)
        {
            ConnectionIndicatorColor = "Green";
            StatusMessage = "Connected";
        }
        else
        {
            ConnectionIndicatorColor = "Gray";
            StatusMessage = IsProvisioning ? "Provisioning complete" : "Disconnected";
        }

        ProvisionWiFiCommand.NotifyCanExecuteChanged();
    }
}

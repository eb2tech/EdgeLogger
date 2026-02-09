# EdgeLogger.Mobile - BLE WiFi Provisioning

## Overview
The EdgeLogger.Mobile app provides Bluetooth LE-based WiFi provisioning for headless Raspberry Pi Zero 2 W devices running EdgeLogger.ApiService.

## Features

### ?? Two Pages

#### 1. **Pi Details Page** (Placeholder)
- Route: `pi-details`
- Currently a placeholder for future Pi status and configuration
- Navigate to WiFi Provisioning page

#### 2. **WiFi Provisioning Page**
- Route: `wifi-provisioning`
- Manual BLE device scanning
- SSID and password entry with validation
- Connection status indicator (colored dot)
- Real-time status messages
- Password visibility toggle
- Provision button (disabled until valid inputs)

---

## Architecture

### Services
- **`IBleProvisioningService`** - Interface for BLE operations
- **`BleProvisioningService`** - Plugin.BLE-based implementation
  - Device scanning and connection
  - GATT characteristic write operations
  - Automatic retry on failed writes (3 attempts)
  - Connection state monitoring

### PageModels (MVVM)
- **`PiDetailsPageModel`** - Placeholder page model
- **`WiFiProvisioningPageModel`** - Main provisioning logic
  - Uses CommunityToolkit.Mvvm for commands and properties
  - Validation logic for SSID/password
  - Connection state handling

### Pages
- **`PiDetailsPage.xaml`** - Placeholder UI
- **`WiFiProvisioningPage.xaml`** - Full provisioning UI

---

## BLE Communication Protocol

### Service UUID
```
12345678-1234-5678-1234-56789abcdef0
```

### Characteristics
| UUID | Purpose | Type |
|------|---------|------|
| `...abcdef1` | SSID | Write |
| `...abcdef2` | Password | Write |
| `...abcdef3` | Command | Write |

### Provisioning Flow
1. User taps **"Scan for Pi"**
2. App scans for device named `"picollect-Setup"`
3. App connects to Pi's BLE service
4. User enters SSID and Password
5. User taps **"Provision WiFi"**
6. App writes SSID ? characteristic `...def1`
7. App writes Password ? characteristic `...def2`
8. App writes `"provision"` ? characteristic `...def3`
9. Pi processes command and connects to WiFi
10. Pi stops advertising (disconnects automatically)

---

## Validation Rules
- **SSID**: Must not be empty
- **Password**: Must not be empty
- **Provision button**: Disabled until:
  - Both fields are valid
  - Connected to Pi
  - Not currently provisioning

---

## Platform Support

### ? Android (Implemented)
- API 21+ (Android 5.0+)
- Bluetooth LE required
- Permissions:
  - `BLUETOOTH`
  - `BLUETOOTH_ADMIN`
  - `BLUETOOTH_SCAN` (Android 12+)
  - `BLUETOOTH_CONNECT` (Android 12+)
  - `ACCESS_FINE_LOCATION` (Android < 12)
  - `ACCESS_COARSE_LOCATION` (Android < 12)
- Runtime permission requests handled in `MainActivity`

### ? iOS (Fully Implemented)
- iOS 15.0+
- Bluetooth LE required
- Info.plist entries configured:
  - `NSBluetoothAlwaysUsageDescription` - "EdgeLogger needs Bluetooth access to configure your Raspberry Pi's WiFi connection via BLE."
  - `NSBluetoothPeripheralUsageDescription` - "EdgeLogger uses Bluetooth to communicate with your Raspberry Pi for WiFi provisioning."
- Permissions requested automatically on first Bluetooth use
- No additional code needed (Plugin.BLE handles iOS automatically)

---

## Dependencies

### NuGet Packages
- **Plugin.BLE** (3.1.0) - Bluetooth LE communication
- **CommunityToolkit.Mvvm** (8.3.2) - MVVM framework (already present)
- **CommunityToolkit.Maui** (11.1.1) - MAUI helpers (already present)

---

## Usage

### For End Users
1. Power on the Raspberry Pi (must be in provisioning mode - no WiFi configured)
2. Open EdgeLogger.Mobile app
3. Navigate to **"Pi Setup"** tab
4. Tap **"Configure WiFi"**
5. Tap **"Scan for Pi"** and wait for connection
6. Enter your WiFi network name (SSID)
7. Enter your WiFi password
8. Tap **"Provision WiFi"**
9. Wait for success message
10. Pi will automatically connect to your network

### Status Indicators
| Color | Meaning |
|-------|---------|
| ?? Gray | Not connected |
| ?? Orange | Scanning... |
| ?? Green | Connected |
| ?? Red | Error/Not found |

---

## Error Handling
- **Bluetooth unavailable** ? Alert dialog prompts user to enable Bluetooth
- **Pi not found** ? Alert dialog with retry instructions
- **Provisioning failed** ? Alert dialog with error message
- **Write failures** ? Automatic retry (3 attempts with 500ms delay)

---

## Connection Management
- **Auto-disconnect**: Pi stops advertising after successful WiFi connection
- **Stay connected**: App maintains connection until Pi disconnects
- **Retry logic**: Failed writes are retried up to 3 times

---

## Future Enhancements (Pi Details Page)
- Display Pi device information
- Show current network status
- Configuration history
- System diagnostics
- Device management tools

---

## Development Notes

### Testing Tips
1. Ensure Pi is powered and in provisioning mode
2. Check Bluetooth is enabled on mobile device
3. Grant all required permissions
4. Pi must be advertising as "picollect-Setup"
5. Valid WiFi credentials required for successful provisioning

### Debugging
- Check Android Logcat for Plugin.BLE debug output
- Enable BLE scanning logs in `BleProvisioningService`
- Monitor Pi logs in `/var/log/` or via serial console

---

## Code Structure
```
EdgeLogger.Mobile/
??? Services/
?   ??? IBleProvisioningService.cs
?   ??? BleProvisioningService.cs
??? PageModels/
?   ??? PiDetailsPageModel.cs
?   ??? WiFiProvisioningPageModel.cs
??? Pages/
?   ??? PiDetailsPage.xaml
?   ??? PiDetailsPage.xaml.cs
?   ??? WiFiProvisioningPage.xaml
?   ??? WiFiProvisioningPage.xaml.cs
??? Converters/
?   ??? InvertedBoolConverter.cs
??? Platforms/
    ??? Android/
        ??? AndroidManifest.xml
        ??? MainActivity.cs
```

---

## License
Part of EdgeLogger project. See main repository for license details.


---


## **Project Overview**
**EdgeLogger.ApiService** is a long‑running .NET service designed for **Raspberry Pi devices running Linux**. It performs three major responsibilities:

1. **Provisioning**  
   - Detects when the Pi is not connected to Wi‑Fi  
   - Starts a **Bluetooth LE (BLE) provisioning server**  
   - Receives Wi‑Fi credentials from a mobile app  
   - Writes a NetworkManager `.nmconnection` file  
   - Triggers NetworkManager to join the user’s Wi‑Fi network  

2. **Device State Management**  
   - Monitors NetworkManager state via **D‑Bus**  
   - Exposes a simple internal state machine  
   - Controls LED status patterns based on device state  
   - Starts/stops provisioning services as needed  

3. **NATS Log Capture**  
   - Connects to a local NATS server  
   - Subscribes to CYD‑generated messages  
   - Writes logs to a local database  
   - Runs only when the Pi has a valid network connection  

The service is intended to run as a **systemd daemon** on Raspberry Pi OS (64‑bit).

---

## **Key Technologies**

### **1. D‑Bus (via Tmds.DBus.Protocol)**
The service uses `Tmds.DBus.Protocol` (AOT‑friendly, no reflection) to communicate with:

- **NetworkManager**  
  - Detect Wi‑Fi connectivity  
  - Watch state changes  
  - Trigger connection attempts  
  - Query device status  

- **BlueZ**  
  - BLE advertising  
  - GATT service registration  
  - Characteristic read/write/notify  

Copilot should generate D‑Bus interfaces using the `Tmds.DBus.Protocol` style:

- Raw `uint` values for enums  
- Explicit interface definitions  
- Async methods  
- No reflection-based attributes  

---

### **2. BLE GATT Server (via BlueZ.NET.Server)**
BLE provisioning is implemented using **BlueZ.NET.Server**, which provides:

- BLE advertising  
- GATT service creation  
- Characteristics with read/write/notify  
- Event handlers for provisioning commands  

Copilot should generate:

- A provisioning service UUID  
- Characteristics for SSID, password, command, and status  
- Async handlers for write requests  
- Notification logic for provisioning status  

---

### **3. NetworkManager Wi‑Fi Configuration**
Wi‑Fi provisioning uses NetworkManager’s standard `.nmconnection` files.

Copilot should generate files like:

```
/etc/NetworkManager/system-connections/homewifi.nmconnection
```

With sections:

```
[connection]
[wifi]
[wifi-security]
[ipv4]
[ipv6]
```

After writing the file, the service must:

- `nmcli connection reload`
- `nmcli connection up homewifi`

Or use NetworkManager D‑Bus equivalents.

---

### **4. Background Services**
The service uses multiple `BackgroundService` classes:

#### **NetworkStateMonitorService**
- Watches NetworkManager state  
- Publishes device state (Disconnected, Connecting, Connected)  
- Starts/stops BLE provisioning  

#### **BleProvisioningService**
- Runs only when Wi‑Fi is disconnected  
- Hosts BLE GATT server  
- Receives credentials  
- Calls WifiConfiguratorService  
- Sends provisioning status notifications  

#### **WifiConfiguratorService**
- Writes `.nmconnection` files  
- Triggers NetworkManager to connect  
- Reports success/failure  

#### **LedStatusService**
- Controls LED patterns based on device state  
- Uses sysfs or libgpiod  

#### **AuraLogMessageService**
- Starts only when Wi‑Fi is connected  
- Subscribes to NATS  
- Writes logs to local storage  

Copilot should follow this modular pattern.

---

## **Device State Model**
Copilot should use a simple enum:

```csharp
public enum DeviceState
{
    Unknown,
    NoWifi,
    Provisioning,
    Connecting,
    Connected,
    Error
}
```

NetworkManager’s raw `uint` state values map to this enum.

---

## **NetworkManager State Values**
Copilot should use these values:

| Value | Meaning |
|-------|---------|
| 0 | Unknown |
| 10 | Asleep |
| 20 | Disconnected |
| 30 | Disconnecting |
| 40 | Connecting |
| 50 | ConnectedLocal |
| 60 | ConnectedSite |
| 70 | ConnectedGlobal |

---

## **BLE Provisioning GATT Model**

### **Service UUID**
```
12345678-1234-5678-1234-56789abcdef0
```

### **Characteristics**
| UUID | Purpose |
|------|---------|
| `...ef01` | SSID (write) |
| `...ef02` | Password (write) |
| `...ef03` | Command (“provision”) |
| `...ef04` | Status (notify) |

Copilot should generate:

- Write handlers  
- Status notifications  
- BLE advertising logic  

---

## **LED Status Patterns**
Copilot should generate LED logic such as:

- Fast blink → provisioning  
- Slow blink → connecting  
- Solid → connected  
- Pulse → error  

---

## **Systemd Integration**
Copilot should generate:

- A `.service` file  
- `WantedBy=multi-user.target`  
- `After=network-online.target`  

---

## **Coding Style Expectations**
Copilot should:

- Use async/await everywhere  
- Avoid reflection  
- Use dependency injection  
- Keep services isolated  
- Use channels or shared state for inter-service communication  
- Prefer D-Bus over shell commands when possible  
- Use structured logging  

---

## **What Copilot Should Avoid**
- Generating Windows-specific code  
- Using Bluetooth APIs from Windows or macOS  
- Using reflection-based D-Bus libraries  
- Using obsolete BlueZ wrappers  
- Using AP-mode provisioning (BLE is primary)  
- Writing `.nmconnection` files outside `/etc/NetworkManager/system-connections/`  

---


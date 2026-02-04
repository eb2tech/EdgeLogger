namespace EdgeLogger.ApiService.Services;

internal enum NetworkState : uint
{
    Unknown = 0, // NetworkManager cannot determine state
    Asleep = 10, // Networking is disabled or system is suspending
    Disconnected = 20, // No active network connection
    Disconnecting = 30, // Disconnecting from a network
    Connecting = 40, // Connecting to a network
    ConnectedLocal = 50, // Connected, but only local link connectivity
    ConnectedSite = 60, // Connected to LAN, but no internet access
    ConnectedGlobal = 70 // Fully connected to network with internet access
}
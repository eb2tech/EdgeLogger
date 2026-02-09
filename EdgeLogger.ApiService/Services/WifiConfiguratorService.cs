using System.Diagnostics;
using System.Text;

namespace EdgeLogger.ApiService.Services;

internal class WifiConfiguratorService(ILogger<WifiConfiguratorService> logger) : IWifiConfigurator
{
    private const string ConnectionsPath = "/etc/NetworkManager/system-connections";
    private const string ConnectionName = "EdgeLogger-WiFi";

    public async Task<bool> ConfigureAndConnectAsync(string ssid, string password, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Configuring WiFi for SSID: {Ssid}", ssid);

            // Generate .nmconnection file content
            var connectionId = Guid.NewGuid().ToString();
            var nmConnectionContent = GenerateNmConnectionFile(ssid, password, connectionId);

            // Write the file
            var filePath = Path.Combine(ConnectionsPath, $"{ConnectionName}.nmconnection");
            await File.WriteAllTextAsync(filePath, nmConnectionContent, cancellationToken);

            // Set correct permissions (600)
            await ExecuteCommandAsync($"chmod 600 {filePath}", cancellationToken);
            logger.LogInformation("WiFi configuration file written and permissions set");

            // Reload NetworkManager connections
            await ExecuteCommandAsync("nmcli connection reload", cancellationToken);
            logger.LogInformation("NetworkManager connections reloaded");

            // Attempt to connect
            var result = await ExecuteCommandAsync($"nmcli connection up {ConnectionName}", cancellationToken);

            if (result.ExitCode == 0)
            {
                logger.LogInformation("Successfully connected to WiFi network: {Ssid}", ssid);
                return true;
            }
            else
            {
                logger.LogError("Failed to connect to WiFi network: {Ssid}. Error: {Error}", ssid, result.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while configuring WiFi for SSID: {Ssid}", ssid);
            return false;
        }
    }

    private static string GenerateNmConnectionFile(string ssid, string password, string uuid)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[connection]");
        sb.AppendLine($"id={ConnectionName}");
        sb.AppendLine($"uuid={uuid}");
        sb.AppendLine("type=wifi");
        sb.AppendLine("autoconnect=true");
        sb.AppendLine("autoconnect-priority=999");
        sb.AppendLine();
        sb.AppendLine("[wifi]");
        sb.AppendLine($"ssid={ssid}");
        sb.AppendLine("mode=infrastructure");
        sb.AppendLine();
        sb.AppendLine("[wifi-security]");
        sb.AppendLine("key-mgmt=wpa-psk");
        sb.AppendLine($"psk={password}");
        sb.AppendLine();
        sb.AppendLine("[ipv4]");
        sb.AppendLine("method=auto");
        sb.AppendLine();
        sb.AppendLine("[ipv6]");
        sb.AppendLine("method=auto");
        sb.AppendLine("addr-gen-mode=default");

        return sb.ToString();
    }

    private async Task<(int ExitCode, string Output, string Error)> ExecuteCommandAsync(
        string command, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
                      {
                          FileName = "/bin/bash",
                          Arguments = $"-c \"{command}\"",
                          RedirectStandardOutput = true,
                          RedirectStandardError = true,
                          UseShellExecute = false,
                          CreateNoWindow = true
                      };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;

            return (process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute command: {Command}", command);
            return (-1, string.Empty, ex.Message);
        }
    }
}
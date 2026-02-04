using System.Runtime.InteropServices;
using System.Text;

namespace EdgeLogger.ApiService.Services;

public static class PiIntrinsics
{
    public static bool IsRunningOnPi()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return false;
        }

        string? model = null;
        const string modelPath = "/proc/device-tree/model";
        if (File.Exists(modelPath))
        {
            try
            {
                var bytes = File.ReadAllBytes(modelPath);
                model = Encoding.UTF8.GetString(bytes).Trim('\0', '\r', '\n');
            }
            catch
            {
                // ignore and fall back
            }
        }

        if (string.IsNullOrEmpty(model))
        {
            try
            {
                var cpuInfo = File.ReadAllText("/proc/cpuinfo");
                foreach (var line in cpuInfo.Split('\n'))
                {
                    if (line.StartsWith("Model", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Hardware", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("model name", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            model = parts[1].Trim();
                            break;
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        var isPi = !string.IsNullOrEmpty(model) && (
            model.Contains("Raspberry", StringComparison.OrdinalIgnoreCase) ||
            model.Contains("BCM") ||
            model.Contains("RPI", StringComparison.OrdinalIgnoreCase)
        );

        return isPi;
    }
}
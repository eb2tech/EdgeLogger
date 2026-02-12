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

    public static void SetLedStateFastBlink()
    {
        // Fast blink: 100ms on/off
        SetLedPattern("timer", delayOn: 100, delayOff: 100);
    }

    public static void SetLedStateSlowBlink()
    {
        // Slow blink: 500ms on/off
        SetLedPattern("timer", delayOn: 500, delayOff: 500);
    }

    public static void SetLedStatePulse()
    {
        // Heartbeat pattern (kernel-managed pulse)
        SetLedPattern("heartbeat");
    }

    public static void SetLedStateNormal()
    {
        // Restore default SD card activity trigger
        SetLedPattern("mmc0");
    }

    private static void SetLedPattern(string trigger, int? delayOn = null, int? delayOff = null)
    {
        // Raspberry Pi Zero 2 W Activity LED paths
        const string ledPath = "/sys/class/leds/led0";
        const string ledPathAlt = "/sys/class/leds/ACT"; // Alternative path on some models

        var activeLedPath = Directory.Exists(ledPath) ? ledPath : ledPathAlt;

        try
        {
            // Set the LED trigger pattern
            File.WriteAllText($"{activeLedPath}/trigger", trigger);

            // Set timing parameters if specified (for timer trigger)
            if (delayOn.HasValue)
            {
                File.WriteAllText($"{activeLedPath}/delay_on", delayOn.Value.ToString());
            }

            if (delayOff.HasValue)
            {
                File.WriteAllText($"{activeLedPath}/delay_off", delayOff.Value.ToString());
            }
        }
        catch
        {
            // Silently ignore errors (not running on Pi, insufficient permissions, etc.)
        }
    }
}
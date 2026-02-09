using Android.App;
using Android.Content.PM;
using Android.OS;

namespace EdgeLogger.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Request Bluetooth and Location permissions at runtime for Android 12+
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // API 31+
        {
            RequestPermissions(new[]
            {
                Android.Manifest.Permission.BluetoothScan,
                Android.Manifest.Permission.BluetoothConnect
            }, 1);
        }
        else
        {
            RequestPermissions(new[]
            {
                Android.Manifest.Permission.AccessFineLocation,
                Android.Manifest.Permission.AccessCoarseLocation
            }, 1);
        }
    }
}


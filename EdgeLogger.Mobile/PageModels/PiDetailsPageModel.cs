using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EdgeLogger.Mobile.PageModels;

public partial class PiDetailsPageModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToProvisioning()
    {
        await Shell.Current.GoToAsync("wifi-provisioning");
    }
}

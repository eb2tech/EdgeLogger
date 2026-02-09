using EdgeLogger.Mobile.PageModels;

namespace EdgeLogger.Mobile.Pages;

public partial class WiFiProvisioningPage : ContentPage
{
    public WiFiProvisioningPage(WiFiProvisioningPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}

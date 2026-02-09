using EdgeLogger.Mobile.PageModels;

namespace EdgeLogger.Mobile.Pages;

public partial class PiDetailsPage : ContentPage
{
    public PiDetailsPage(PiDetailsPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}

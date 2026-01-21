using EdgeLogger.Mobile.Models;
using EdgeLogger.Mobile.PageModels;

namespace EdgeLogger.Mobile.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}
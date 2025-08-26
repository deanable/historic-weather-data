using HistoricWeatherData.Core.ViewModels;

namespace HistoricWeatherData.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
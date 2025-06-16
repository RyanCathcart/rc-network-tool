using rc_network_tool.ViewModels;

namespace rc_network_tool.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _mainViewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        _mainViewModel = viewModel;
        BindingContext = _mainViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _mainViewModel.LoadMacOuiRegistryAsync();
    }
}

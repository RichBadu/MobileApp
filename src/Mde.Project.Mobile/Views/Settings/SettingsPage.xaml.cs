using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Settings;

public partial class SettingsPage : ContentPage
{
	private readonly SettingsViewModel _viewModel;
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUserDataCommand.ExecuteAsync(null);
    }
}
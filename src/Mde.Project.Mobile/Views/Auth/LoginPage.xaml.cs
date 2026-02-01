using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Auth;

public partial class LoginPage : ContentPage
{
	private readonly LoginViewModel _viewModel;
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
        BindingContext = viewModel;
    }
}
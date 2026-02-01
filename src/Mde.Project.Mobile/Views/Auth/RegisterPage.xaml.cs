using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Auth;

public partial class RegisterPage : ContentPage
{
	private readonly RegisterViewModel _registerViewModel;
	public RegisterPage(RegisterViewModel registerViewModel)
	{
		InitializeComponent();
		_registerViewModel = registerViewModel;
		BindingContext = _registerViewModel;

	}
}
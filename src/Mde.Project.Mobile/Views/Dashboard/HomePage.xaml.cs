using Mde.Project.Mobile.ViewModels;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Views.Dashboard;

public partial class HomePage : ContentPage
{
	private readonly HomeViewModel _homeViewModel;
	public HomePage(HomeViewModel homeViewModel)
	{
		InitializeComponent();
		_homeViewModel = homeViewModel;
		BindingContext = _homeViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		 _homeViewModel.LoadHomePageCommand.Execute(null);
    }

}
using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Categories;

public partial class CategoriesPage : ContentPage
{
	private readonly CategoriesViewModel _viewModel;
	public CategoriesPage(CategoriesViewModel categoryViewModel)
	{
		InitializeComponent();
		_viewModel = categoryViewModel;
		BindingContext = _viewModel;
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
		_viewModel.LoadCategoriesCommand.Execute(null);
    }
}
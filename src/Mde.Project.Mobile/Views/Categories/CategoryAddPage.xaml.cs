using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Categories;

public partial class CategoryAddPage : ContentPage
{
	private readonly CategoryAddViewModel _viewModel;
    public CategoryAddPage(CategoryAddViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
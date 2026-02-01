using Mde.Project.Mobile.ViewModels;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Views.Categories;

public partial class CategoryDetailPage : ContentPage
{
	private readonly CategoryDetailViewModel _vievModel;
    public CategoryDetailPage(CategoryDetailViewModel vievModel)
    {
        InitializeComponent();
        _vievModel = vievModel;
        BindingContext = _vievModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
       await _vievModel.LoadCategoryCommand.ExecuteAsync(null);
    }
}
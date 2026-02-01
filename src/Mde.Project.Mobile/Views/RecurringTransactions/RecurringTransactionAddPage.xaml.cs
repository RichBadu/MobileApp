using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.RecurringTransactions;

public partial class RecurringTransactionAddPage : ContentPage
{
	private readonly RecurringTransactionAddViewModel _viewModel;
    public RecurringTransactionAddPage(RecurringTransactionAddViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}
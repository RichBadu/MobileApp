using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Transactions;

public partial class TransactionAddPage : ContentPage
{
	private readonly TransactionAddViewModel _transactionAddViewModel;
    public TransactionAddPage(TransactionAddViewModel transactionAddViewModel)
    {
        InitializeComponent();
        _transactionAddViewModel = transactionAddViewModel;
        BindingContext = _transactionAddViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _transactionAddViewModel.LoadCategoriesCommand.ExecuteAsync(null);
    }
}
using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.Transactions;

public partial class TransactionPage : ContentPage
{
	private readonly TransactionViewModel _transactionViewModel;
	public TransactionPage(TransactionViewModel transactionViewModel)
	{
		InitializeComponent();
		_transactionViewModel = transactionViewModel;
		BindingContext = _transactionViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
	   _transactionViewModel.LoadTransactionCommand.Execute(null);
    }
}
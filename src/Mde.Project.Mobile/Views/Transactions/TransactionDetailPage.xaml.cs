using Mde.Project.Mobile.ViewModels;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Views.Transactions;

public partial class TransactionDetailPage : ContentPage
{
	private readonly TransactionDetailViewModel _transactionDetailViewModel;
    public TransactionDetailPage(TransactionDetailViewModel transactionDetailViewModel)
    {
        InitializeComponent();
        _transactionDetailViewModel = transactionDetailViewModel;
        BindingContext = _transactionDetailViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _transactionDetailViewModel.LoadTransactionCommand.ExecuteAsync(null);
    }
}
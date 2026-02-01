using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.ViewModels;

namespace Mde.Project.Mobile.Views.RecurringTransactions;

public partial class RecurringTransactionsPage : ContentPage
{
	private readonly RecurringTransactionViewModel _viewModel;
   
    public RecurringTransactionsPage(RecurringTransactionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
         _viewModel.LoadRecurringTransactionsCommand.Execute(null);
        
    }

}
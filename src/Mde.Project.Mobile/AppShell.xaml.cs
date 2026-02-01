using Mde.Project.Mobile.Views.Auth;
using Mde.Project.Mobile.Views.Categories;
using Mde.Project.Mobile.Views.RecurringTransactions;
using Mde.Project.Mobile.Views.Transactions;

namespace Mde.Project.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(TransactionDetailPage), typeof(TransactionDetailPage));
            Routing.RegisterRoute(nameof(TransactionAddPage), typeof(TransactionAddPage));

            
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
            Routing.RegisterRoute(nameof(CategoryAddPage), typeof(CategoryAddPage));

            Routing.RegisterRoute(nameof(RecurringTransactionsPage), typeof(RecurringTransactionsPage));
            Routing.RegisterRoute(nameof(RecurringTransactionAddPage), typeof(RecurringTransactionAddPage));



        }
    }
}

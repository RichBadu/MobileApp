using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Core.Helpers;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Views.Categories;
using Mde.Project.Mobile.Views.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ITransactionRecordService _transactionService;
        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly INotificationService _notificationService;
        private readonly ISyncService _syncService;
        private string? UserId => CurrentUser.GetUserId();
        private bool _notificationsSetup = false;

        public HomeViewModel(
      IUserService userService,
      ICategoryService categoryService,
      ITransactionRecordService transactionService,
      INotificationService notificationService,
      IRecurringTransactionService recurringTransactionService,
      ISyncService syncService)
        {
            _userService = userService;
            _categoryService = categoryService;
            _transactionService = transactionService;
            _notificationService = notificationService;
            _recurringTransactionService = recurringTransactionService;
            _syncService = syncService;
        }

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private decimal totalBalance = 0m;

        [ObservableProperty]
        private decimal monthlyIncome = 0m;

        [ObservableProperty]
        private decimal monthlyExpenses = 0m;

        [ObservableProperty]
        private string currentMonth = DateTime.Now.ToString("MMMM yyyy");

        [ObservableProperty]
        private int selectedMonth = DateTime.Now.Month;

        [ObservableProperty]
        private int selectedYear = DateTime.Now.Year;

        [ObservableProperty]
        private bool isSyncing = false;

        [ObservableProperty]
        private string syncStatus = "";

        [ObservableProperty]
        private bool hasCategories = false;

        [ObservableProperty]
        private bool hasTransactions = false;

        public ObservableCollection<CategoryWithSpending> TopCategories { get; } = new();
        public ObservableCollection<TransactionRecord> RecentTransactions { get; } = new();



        [RelayCommand]
        async Task LoadHomePage()
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(UserId);
                if (user != null)
                {
                    UserName = user.DisplayName ?? "User";
                }
                await SetupNotificationsAsync();

                await LoadMonthlyData();

                await LoadTopCategories();

                await LoadRecentTransactions();
               
                var processedCount = await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(UserId);
                if (processedCount > 0)
                {
                    await Shell.Current.DisplaySnackbar($"{processedCount} recurring transactions processed");
                }



            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load home page: {ex.Message}","Ok");
            }
        }

        [RelayCommand]
        async Task PreviousMonth()
        {
            SelectedMonth--;
            if(SelectedMonth < 1)
            {
                SelectedMonth = 12;
                SelectedYear--;
            }

            UpdateCurrentMonth();
            await LoadMonthlyData();
            await LoadTopCategories();
            await LoadRecentTransactions();
        }

        [RelayCommand]
        async Task NextMonth()
        {
            SelectedMonth++;
            if(SelectedMonth > 12)
            {
                SelectedMonth = 1;
                SelectedYear++;        
            }
            UpdateCurrentMonth();
            await LoadMonthlyData();
            await LoadTopCategories();
            await LoadRecentTransactions();
        }

        [RelayCommand]
        async Task AddTransaction()
        {
            await Shell.Current.GoToAsync(nameof(TransactionAddPage));
        }
        [RelayCommand]
        async Task ViewAllCategories()
        {
            //navigate to view all categories
            await Shell.Current.GoToAsync(nameof(CategoriesPage));
        }

        [RelayCommand]
        async Task ViewAllTranscations()
        {
            //navigate to view all transactions
            await Shell.Current.GoToAsync("//transactions");
        }
       
        [RelayCommand]
        async Task NavigateToProfile()
        {
            //naviagte to settings page
            await Shell.Current.GoToAsync("//settings");          
        }

        private void UpdateCurrentMonth()
        {
            var date = new DateTime(SelectedYear, SelectedMonth, 1);
            CurrentMonth = date.ToString("MMMM yyyy");
        }

        private async Task LoadRecentTransactions()
        {
            var allMonthTransactions = await _transactionService.GetTransactionsByMonthAsync(
                   UserId,
                   SelectedMonth,
                   SelectedYear
   );

            RecentTransactions.Clear();

            // Take top 3 most recent from that month
            var recentFromMonth = allMonthTransactions
                .OrderByDescending(t => t.Date)
                .Take(3);

            foreach (var transaction in recentFromMonth)
            {
                RecentTransactions.Add(transaction);
            }

            HasTransactions = RecentTransactions.Any();

        }

        private async Task LoadTopCategories()
        {
            try
            {
                // Get expenses grouped by category
                var expensesByCategory = await _transactionService.GetExpensesByCategoryAsync(
                    UserId,
                    SelectedMonth,
                    SelectedYear
                );

                TopCategories.Clear();

                // Get top 5 categories by spending
                var topCategoryIds = expensesByCategory
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .Select(x => x.Key)
                    .ToList();

              
                foreach (var categoryId in topCategoryIds)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(categoryId);

                    if (category != null)
                    {
                        var spent = expensesByCategory[categoryId];  

                        TopCategories.Add(new CategoryWithSpending
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Icon = category.Icon,
                            Color = category.Color,
                            MonthlyBudget = category.MonthlyBudget ?? 0,
                            SpentThisMonth = spent  
                        });
                    }
                }

               
                HasCategories = TopCategories.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top categories: {ex.Message}");
                HasCategories = false;
            }
        }

        private async Task LoadMonthlyData()
        {
            TotalBalance = await _transactionService.GetBalanceAsync(UserId, SelectedMonth, SelectedYear);
            MonthlyIncome = await _transactionService.GetTotalIncomeAsync(UserId, SelectedMonth,SelectedYear);
           MonthlyExpenses = await _transactionService.GetTotalExpensesAsync(UserId, SelectedMonth,SelectedYear);
        }

        private async Task SetupNotificationsAsync()
        {
            if (_notificationsSetup)
                return;

#if WINDOWS
    _notificationsSetup = true;
    System.Diagnostics.Debug.WriteLine("⚠️ Notifications skipped on Windows");
    return;
#endif
            var userDisabledNotifications = Preferences.ContainsKey("NotificationsEnabled") &&
                                !Preferences.Get("NotificationsEnabled", true);

            if (userDisabledNotifications)
            {
                _notificationsSetup = true;
                return;
            }


            var permission = await _notificationService.RequestPermissionsAsync();

            if (permission)
            {
                await _notificationService.ScheduleDailyReminderAsync();

                if (!Preferences.ContainsKey("NotificationsEnabled"))
                {
                    Preferences.Set("NotificationsEnabled", true);
                    Preferences.Set("DailyReminderEnabled", true);
                    Preferences.Set("BudgetwarningEnabled", true);
                }
             
            }
            _notificationsSetup = true;
        }
    }
}

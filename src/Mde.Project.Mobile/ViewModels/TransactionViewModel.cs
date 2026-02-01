using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Views.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class TransactionViewModel : ObservableObject
    {
        private readonly ITransactionRecordService _transactionService;
        private readonly IRecurringTransactionService _recurringTransactionService;
        private string? UserId => CurrentUser.GetUserId();
        private bool _initialized = false;
        private bool _isloading = false;    
        public TransactionViewModel(ITransactionRecordService transactionService, IRecurringTransactionService recurringTransactionService)
        {
            _transactionService = transactionService;
            _recurringTransactionService = recurringTransactionService;
            _ = CheckPendingRecurringAsync();
        }

        [ObservableProperty]
        private string selectedFilter = "All";
        [ObservableProperty]
        private bool isRefreshing;
        [ObservableProperty]
        private bool hasPendingRecurring;

        [ObservableProperty]
        private ObservableCollection<TransactionGroup> groupedTransactions = new();

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            _initialized = true;

            await CheckPendingRecurringAsync();
        //    await LoadTransaction();
        }


        [RelayCommand]
        async Task AddTransaction()
        {
            await Shell.Current.GoToAsync(nameof(TransactionAddPage));
        }
        [RelayCommand]
        async Task RefreshDataAsync()
        {
            if (_isloading)
                return;

            try
            {
                IsRefreshing = true;
                await LoadTransaction();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task SetFilter(string filter)
        {
            SelectedFilter = filter;
            await LoadTransaction();
        }

        [RelayCommand]
        async Task LoadTransaction()
        {
            if (_isloading)
                return;
            try
            {
                _isloading = true;

                var transactions = await _transactionService.GetAllTransactionRecordsAsync(UserId);

                if (SelectedFilter == "Income")
                {
                    transactions = await _transactionService.GetTransactionByTypeAsync(UserId, "Income");
                }else if(SelectedFilter == "Expenses")
                {
                    transactions = await _transactionService.GetTransactionByTypeAsync(UserId, "Expense");
                }
              

                   var groups = transactions
                  .GroupBy(t => GetDateGroupName(t.Date))
                  .OrderBy(g => GetDateGroupOrder(g.Key))
                  .Select(g => new TransactionGroup(g.Key, g.ToList()))
                  .ToList();
            

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    GroupedTransactions.Clear();

                    foreach (var group in groups)
                    {
                        System.Diagnostics.Debug.WriteLine($"   Adding group: {group.DateGroup} ({group.Count} items)");
                        GroupedTransactions.Add(group);
                    }
                });

            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
            finally
            {
                 _isloading = false;
            }
        }

     

        [RelayCommand]
        async Task TransactionTapped(TransactionRecord transaction)
        {
            // TODO: Navigate to transaction detail page
            var route = nameof(TransactionDetailPage);
            await Shell.Current.GoToAsync(route, true, new Dictionary<string, object>
{
                         { "TransactionId", transaction.Id } 
                       });
        
        }

        private async Task CheckPendingRecurringAsync()
        {
            var rec = await _recurringTransactionService.GetDueRecurringTransactionsAsync(UserId);

            HasPendingRecurring = rec.Any();
        }

        private string GetDateGroupName(DateTime date)
        {
            var today = DateTime.Today;

            if (date.Date == today)
                return "TODAY";
            else if (date.Date == today.AddDays(-1))
                return "YESTERDAY";
            else if (date.Date >= today.AddDays(-7))
                return date.ToString("dddd").ToUpper(); 
            else
                return date.ToString("MMMM yyyy").ToUpper(); 
        }

        private int GetDateGroupOrder(string groupName)
        {
            return groupName switch
            {
                "TODAY" => 0,
                "YESTERDAY" => 1,
                _ => 2
            };
        }
    }

}


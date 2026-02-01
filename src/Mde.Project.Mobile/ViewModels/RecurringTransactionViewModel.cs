using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Views.RecurringTransactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class RecurringTransactionViewModel : ObservableObject
    {
        private readonly IRecurringTransactionService _recurringTransactionService;

        private string? UserId => CurrentUser.GetUserId();
        public RecurringTransactionViewModel(
         IRecurringTransactionService recurringTransactionService,
         ICategoryService categoryService)
        {
            _recurringTransactionService = recurringTransactionService;
            
        }

        [ObservableProperty]
        private ObservableCollection<RecurringTransaction> recurringTransactions = new();
        [ObservableProperty]
        private bool isRecurringActive;





        [RelayCommand]
        async Task LoadRecurringTransactions()
        {
            try
            {
                
                List<RecurringTransaction> transaction = new();

                transaction = await _recurringTransactionService.GetAllRecurringTransactionsAsync(UserId);
                RecurringTransactions.Clear();

                foreach(var recurring in transaction.OrderBy(r => r.NextDueDate))
                {
                  
                    RecurringTransactions.Add(recurring);
                }
                


            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error","failed to load recurrent transactions","Ok");
            }
        }



        [RelayCommand]
        async Task AddRecurring()
        {
            await Shell.Current.GoToAsync(nameof(RecurringTransactionAddPage));
        }
        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task DeleteRecurring(RecurringTransaction recurring)
        {
            bool confirm = await Shell.Current.DisplayAlert(
    "Delete Recurring Transaction",
    $"Are you sure you want to delete '{recurring.Name}'?",
    "Delete",
    "Cancel");

            if (!confirm)
                return;

            try
            {
                await _recurringTransactionService.DeleteRecurringTransactionAsync(recurring.Id);
                RecurringTransactions.Remove(recurring);

            
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }

          
        }

      

    }
}

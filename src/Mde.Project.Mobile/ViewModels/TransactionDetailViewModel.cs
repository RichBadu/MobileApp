using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    [QueryProperty(nameof(TransactionRecordId), "TransactionId")]
    public partial class TransactionDetailViewModel : ObservableObject
    {
        private string? UserId => CurrentUser.GetUserId();

        private readonly ITransactionRecordService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IPhotoService _photoService;

        public TransactionDetailViewModel(ITransactionRecordService transactionRecordService, ICategoryService categoryService,
            IPhotoService photoService)
        {
            _transactionService = transactionRecordService;
            _categoryService = categoryService;
            _photoService = photoService;
        }




        [ObservableProperty]
        private bool hasPhoto;

        [ObservableProperty]
        private string transactionRecordId;
       
        [ObservableProperty]
        private TransactionRecord? transaction;
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string type = string.Empty;
        
        [ObservableProperty]
        private DateTime date = DateTime.Now;    

        [ObservableProperty]
        private string? photoPath;

        [ObservableProperty]
        private bool isEditing = false;

        [ObservableProperty]
        private Category? category;

        [ObservableProperty]
        private string formattedAmount = string.Empty;

        [ObservableProperty]
        private string amountColor = "#000000";

        [ObservableProperty]
        private TimeSpan time = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private List<Category> categories;

        [RelayCommand]
        public async Task SaveTransaction()
        {
            if (Transaction == null) return;

            try
            {
                Transaction.Name = Name;    
                Transaction.Description = Description;
                Transaction.Amount = Amount;
                Transaction.CategoryId = Category?.Id ?? Transaction.CategoryId;
                Transaction.Type = Type;
                Transaction.Date = Date.Date + Time;
                Transaction.PhotoPath = PhotoPath;
                Transaction.UpdatedAt = DateTime.Now;

                var updated = await _transactionService.UpdateTransactionAsync(Transaction);

                if(updated != null)
                {
                    await Shell.Current.DisplayAlert("Succes", "transaction updated", "OK");
                    UpdateFormattedAmount();
                    IsEditing = false;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update transaction", "OK");
                }
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task DeleteTransaction()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                 "Delete Transaction",
                "Are you sure you want to delete this transaction?",
                "Delete",
                "Cancel");

            if (confirm && Transaction != null)
            {

                if (!string.IsNullOrEmpty(Transaction.PhotoPath))
                {
                    await _photoService.DeletePhotoAsync(Transaction.PhotoPath);
                }

                var success = await _transactionService.DeleteTransactionAsync(Transaction.Id);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Transaction deleted", "OK");
                    await Shell.Current.GoToAsync("///transactionsTab/transactions");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete transaction", "OK");
                }
            }
        }
       
        [RelayCommand]
        async Task GoBack()
        {
            if (IsEditing)
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Unsaved Changes",
                    "You have unsaved changes. Discard them?",
                    "Discard",
                    "Cancel");

                if (!confirm) return;
            }

            await Shell.Current.GoToAsync("//transactionsTab/transactions");
        }

        [RelayCommand]
        public async Task StartEditing()
        {
            await LoadCategories();
            IsEditing = true;
        }

        [RelayCommand]
        public void CancelEditing()
        {
            if(Transaction != null)
            {
               LoadValuesFromTransaction(Transaction);
            }
            IsEditing = false;
        }

   

        [RelayCommand]
        public async Task LoadTransaction()
        {
            try
            {
                Transaction = await _transactionService.GetTransactionByIdAsync(TransactionRecordId);
                await LoadCategories();

                if (Transaction == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Transaction not found", "OK");
                    await Shell.Current.GoToAsync(nameof(TransactionViewModel));
                    return;
                }

                Category = Categories.FirstOrDefault(c => c.Id == Transaction.CategoryId);
                LoadValuesFromTransaction(Transaction);
                UpdateFormattedAmount();

            } catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load transaction: {ex.Message}", "OK");
            }        
        }
       
        [RelayCommand]
        async Task TakePhoto()
        {
            try
            {
                var path = await _photoService.TakePhotoAsync();

                if (!string.IsNullOrEmpty(path))
                {
                 
                    if (!string.IsNullOrEmpty(PhotoPath))
                    {
                        await _photoService.DeletePhotoAsync(PhotoPath);
                    }

                    PhotoPath = path;
                    HasPhoto = true;
                }
            }
            catch (NotSupportedException ex)
            {
                await Shell.Current.DisplayAlert("Not Supported", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task PickPhoto()
        {
            try
            {
                var path = await _photoService.PickPhotoAsync();

                if (!string.IsNullOrEmpty(path))
                {
                    
                    if (!string.IsNullOrEmpty(PhotoPath))
                    {
                        await _photoService.DeletePhotoAsync(PhotoPath);
                    }

                    PhotoPath = path;
                    HasPhoto = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to pick photo: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task RemovePhoto()
        {
            if (!string.IsNullOrEmpty(PhotoPath))
            {
                await _photoService.DeletePhotoAsync(PhotoPath);
                PhotoPath = null;
                HasPhoto = false;
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                Categories = await _categoryService.GetAllCategoriesAsync(UserId);

            }catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");

            }
        }

        private void LoadValuesFromTransaction(TransactionRecord transaction)
        {

            Name = transaction.Name;
            Description = transaction.Description ?? string.Empty;
            Amount = transaction.Amount;
            Type = transaction.Type;
            Date = transaction.Date;
            PhotoPath = transaction.PhotoPath;
            Time = transaction.Date.TimeOfDay;
            if (!string.IsNullOrEmpty(transaction.PhotoPath))
            {
                var photoDirectory = Path.Combine(FileSystem.AppDataDirectory, "Photos");
                PhotoPath = Path.Combine(photoDirectory, transaction.PhotoPath);

                HasPhoto = File.Exists(PhotoPath);
            }
            else
            {
                PhotoPath = null;
                HasPhoto = false;
            }
        }

        private void UpdateFormattedAmount()
        {
            if (Type == "Income")
            {
                FormattedAmount = $"+ € {Amount:N2}";
                AmountColor = "#4CAF50";
            }
            else
            {
                FormattedAmount = $"- € {Amount:N2}";
                AmountColor = "#FF6B6B";
            }
        }
    }
}

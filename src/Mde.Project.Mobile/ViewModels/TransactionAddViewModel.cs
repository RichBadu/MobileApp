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
    public partial class TransactionAddViewModel : ObservableObject
    {
        private readonly ITransactionRecordService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IPhotoService _photoService;
        private readonly INotificationService _notificationService;
        private string? UserId => CurrentUser.GetUserId();

        public TransactionAddViewModel(
         ITransactionRecordService transactionService,
         ICategoryService categoryService,
         IPhotoService photoService,
         INotificationService notificationService)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;
            _photoService = photoService;
            _notificationService = notificationService;
        }
        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string type = "Expense";

        [ObservableProperty]
        private DateTime date = DateTime.Today;

        [ObservableProperty]
        private TimeSpan time = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private string? photoPath;
       
        [ObservableProperty]
        private bool hasPhoto;

        [ObservableProperty]
        private List<Category> categories = new();

        [ObservableProperty]
        private bool isSaving = false;

       

        [RelayCommand]
        async Task LoadCategories()
        {
            try
            {
                Categories = await _categoryService.GetAllCategoriesAsync(UserId);

                
                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task SaveTransaction()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a transaction name", "OK");
                return;
            }
            if (Amount <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Amount must be greater than 0", "OK");
                return;
            }

            if (SelectedCategory == null)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please select a category", "OK");
                return;
            }


            try
            {
                IsSaving = true;

                string photoFileName = null;
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    photoFileName = Path.GetFileName(PhotoPath);
                }

                var newTransaction = new TransactionRecord
                {
                    UserId = UserId,
                    Name = Name,
                    Description = Description,
                    Amount = Amount,
                    Type = Type,
                    Date = Date.Date + Time,
                    CategoryId = SelectedCategory.Id,
                    PhotoPath = photoFileName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var created = await _transactionService.CreateTransactionRecordAsync(newTransaction);

                if (created != null)
                {
                    await CheckBudgetWarningAsync(newTransaction);
                    await Shell.Current.DisplayAlert("Success", "Transaction added!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to add transaction", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
            }
            finally
            {
                IsSaving = false;
            }
         
        }

        [RelayCommand]
        void SetType(string type)
        {
            Type = type;
        }

        [RelayCommand]
        async Task Cancel()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Cancel",
                "Discard this transaction?",
                "Yes",
                "No");

            if (confirm)
            {
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    await _photoService.DeletePhotoAsync(PhotoPath);
                }
                await Shell.Current.GoToAsync("..");
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

        private async Task CheckBudgetWarningAsync(TransactionRecord transaction)
        {
           
            if (transaction.Type != "Expense")
                return;

            
            var category = SelectedCategory;

            
            if (category?.MonthlyBudget == null || category.MonthlyBudget <= 0)
                return;


            var totalSpent = await _transactionService.GetCategoryMonthlyTotalAsync(
                                 UserId,
                                 category.Id,
                                 DateTime.Now.Month,
                                 DateTime.Now.Year
                                                 );

            
            if (totalSpent > category.MonthlyBudget)
            {
                
                await _notificationService.ShowBudgetWarningAsync(
                    category.Name,
                    totalSpent,
                    category.MonthlyBudget.Value
                );
            }
        }


    }
}

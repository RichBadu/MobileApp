using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class RecurringTransactionAddViewModel : ObservableObject
    {
        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly ICategoryService _categoryService;
        private string? UserId => CurrentUser.GetUserId();

        public RecurringTransactionAddViewModel(
            IRecurringTransactionService recurringTransactionService,
            ICategoryService categoryService)
        {
            _recurringTransactionService = recurringTransactionService;
            _categoryService = categoryService;

            // Set defaults
            SelectedType = "Expense";
            SelectedFrequency = "Daily";
            DayOfMonth = 1;
        }

        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private decimal amount;
        [ObservableProperty]
        private string selectedType = "Expense";
        [ObservableProperty]
        private ObservableCollection<Category> categories = new();
        [ObservableProperty]
        private Category? selectedCategory;
        [ObservableProperty]
        private string selectedFrequency = "Monthly";
        public ObservableCollection<string> Frequencies { get; } = new()
        {
                "Daily",
                "Weekly",
                "Monthly",
                "Yearly"
        };
        [ObservableProperty]
        private int dayOfMonth = 1;

        [ObservableProperty]
        private string selectedDayOfWeek = "Monday";

        public ObservableCollection<string> DaysOfWeek { get; } = new()
        {
            "Monday", "Tuesday", "Wednesday", "Thursday",
            "Friday", "Saturday", "Sunday"
        };

        [ObservableProperty]
        private bool showDayOfMonth;

        [ObservableProperty]
        private bool showDayOfWeek;

        [ObservableProperty]
        private bool isSaving;

        partial void OnSelectedFrequencyChanged(string value)
        {
            ShowDayOfMonth = value == "Monthly" || value == "Yearly";
            ShowDayOfWeek = value == "Weekly";
        }

        [RelayCommand]
        async Task LoadCategories()
        {
            try
            {
                var allCategories = await _categoryService.GetAllCategoriesAsync(UserId);

                Categories.Clear();
                foreach (var category in allCategories)
                {
                    Categories.Add(category);
                }


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
        void SetType(string type)
        {
            SelectedType = type;
        }
        [RelayCommand]
        async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task Save()
        {
            string validator = Validation();
            if (validator != "")
            {
                await Shell.Current.DisplayAlert("Validation", $"{validator}", "OK");
                return;
            }

            IsSaving = true;
            try
            {
                var newRecurring = new RecurringTransaction
                {
                    UserId = UserId,
                    Name = Name,
                    Amount = Amount,
                    Type = SelectedType,
                    CategoryId = SelectedCategory.Id,
                    Frequency = SelectedFrequency,
                    DayOfMonth = ShowDayOfMonth ? DayOfMonth : null,
                    DayOfWeek = ShowDayOfWeek ? GetDayOfWeekNumber(SelectedDayOfWeek) : null,
                    IsActive = true,
                };
                var result = await _recurringTransactionService.CreateRecurringTransactionAsync(newRecurring);
                if (result != null)
                {
                    await Shell.Current.DisplayAlert("Success", "Recurring transaction created!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to create recurring transaction", "OK");
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

        private string Validation()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "Name required";
            }

            if (Amount <= 0)
            {
                return "Amount has to be higher than 0";
            }

            if (SelectedCategory == null)
            {
                return "Select a category";
            }
            if (ShowDayOfMonth && (DayOfMonth < 1 || DayOfMonth > 31))
            {
                return "Pick between 1 and 31";
            }
            return "";
        }

        private int GetDayOfWeekNumber(string dayName)
        {
            return dayName switch
            {
                "Monday" => 1,
                "Tuesday" => 2,
                "Wednesday" => 3,
                "Thursday" => 4,
                "Friday" => 5,
                "Saturday" => 6,
                "Sunday" => 0,
                _ => 1
            };

        }
    }
}

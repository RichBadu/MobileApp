using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Views.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    [QueryProperty(nameof(CategoryId), "CategoryId")]
    public partial class CategoryDetailViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private readonly ITransactionRecordService _transactionService;
        private string? UserId => CurrentUser.GetUserId();

        public CategoryDetailViewModel(
            ICategoryService categoryService,
            ITransactionRecordService transactionService)
        {
            _categoryService = categoryService;
            _transactionService = transactionService;
        }

        [ObservableProperty]
        private string categoryId;

        [ObservableProperty]
        private Category? category;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string icon = string.Empty;

        [ObservableProperty]
        private string color = "#FF6B6B";

        [ObservableProperty]
        private decimal? monthlyBudget;

        [ObservableProperty]
        private bool isEditing = false;
    
        [ObservableProperty]
        private string type = "Expense";

        public List<string> PresetColors { get; } = new()
        {
            "#FF6B6B", // Red
            "#4CAF50", // Green
            "#2196F3", // Blue
            "#FFC107", // Yellow
            "#9C27B0", // Purple
            "#FF9800", // Orange
            "#00BCD4", // Cyan
            "#E91E63", // Pink
            "#795548", // Brown
            "#607D8B"  // Blue Grey
        };


        [RelayCommand]
        void StartEditing()
        {
            IsEditing = true;
        }

        [RelayCommand]
        void CancelEditing()
        {
            if (Category != null)
            {
                LoadValuesFromCategory();
            }
            IsEditing = false;
        }

        [RelayCommand]
        void SetColor(string colorHex)
        {
            Color = colorHex;
        }

        [RelayCommand]
        async Task LoadCategory()
        {
            try
            {
                Category = await _categoryService.GetCategoryByIdAsync(CategoryId);
                if (Category == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Category not found", "OK");
                    await Shell.Current.GoToAsync(nameof(CategoriesPage));
                    return;
                }

                LoadValuesFromCategory();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load category: {ex.Message}", "OK");

            }
        }

        [RelayCommand]
        async Task SaveCategory()
        {
            if (Category == null) return;

            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a category name", "OK");
                return;
            }
            if (MonthlyBudget < 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Budget cannot be negative", "OK");
                return;
            }

            try
            {
                Category.Name = Name;
                Category.Icon = Icon is " " ? "default" : Icon;
                Category.Color = Color;
                Category.MonthlyBudget = MonthlyBudget;
                Category.Type = Type;

                var updated = await _categoryService.UpdateCategoryAsync(Category);

                if (updated != null)
                {
                    await Shell.Current.DisplayAlert("Success", "Category updated!", "OK");
                    IsEditing = false;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to update category", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
            }

        }

        [RelayCommand]
        async Task DeleteCategory()
        {
            if (Category == null) return;
            var otherCategory = await _categoryService.GetCategoryByName(UserId, "Other");

            if (Category.Id == otherCategory.Id) 
            {
                await Shell.Current.DisplayAlert(
                    "Cannot Delete",
                    "The 'Other' category cannot be deleted as it's used as a default category.",
                    "OK");
                return;
            }

            var transactions = await _transactionService.GetAllTransactionRecordsAsync(UserId);
            var hasTransactions = transactions.Any(t => t.CategoryId == Category.Id);

            if (hasTransactions)
            {
                var transactionCount = transactions.Count(t => t.CategoryId == Category.Id);

                var choice = await Shell.Current.DisplayActionSheet(
                    $"This category has {transactionCount} transaction(s). What do you want to do?",
                    "Cancel",
                    null,
                    "Move to 'Other' category",
                    "Delete all transactions");

                if (choice == "Move to 'Other' category")
                {

                    var reassigned = await _transactionService.ReassignTransactionsAsync(Category.Id, otherCategory.Id);

                    if (!reassigned)
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to reassign transactions", "OK");
                        return;
                    }
                }
                else if (choice == "Delete all transactions")
                {
                    bool confirmDelete = await Shell.Current.DisplayAlert(
                        "Confirm Delete",
                        $"Are you sure you want to delete {transactionCount} transaction(s)? This cannot be undone.",
                        "Delete",
                        "Cancel");

                    if (!confirmDelete)
                        return;

                    var deleted = await _transactionService.DeleteTransactionsByCategoryAsync(Category.Id);

                    if (!deleted)
                    {
                        await Shell.Current.DisplayAlert("Error", "Failed to delete transactions", "OK");
                        return;
                    }
                }
                else
                {
                    
                    return;
                }
            }

            
            bool confirm = await Shell.Current.DisplayAlert(
                "Delete Category",
                $"Are you sure you want to delete '{Category.Name}'?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                var success = await _categoryService.DeleteCategoryAsync(Category.Id);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Category deleted", "OK");
                    await Shell.Current.GoToAsync(nameof(CategoriesPage));
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete category", "OK");
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

            await Shell.Current.GoToAsync(nameof(CategoriesPage));

        }
        private void LoadValuesFromCategory()
        {
            if (Category == null) return;

            Name = Category.Name;
            Icon = Category.Icon;
            Color = Category.Color;
            MonthlyBudget = Category.MonthlyBudget;
        }
    }
}

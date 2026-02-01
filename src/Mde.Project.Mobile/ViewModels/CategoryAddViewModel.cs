using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
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
    public partial class CategoryAddViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private string? UserId => CurrentUser.GetUserId();
        public CategoryAddViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string icon = string.Empty;

        [ObservableProperty]
        private string color = "#FF6B6B";

        [ObservableProperty]
        private decimal? monthlyBudget;

        [ObservableProperty]
        private bool isSaving = false;
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
        async Task SaveCategory()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a category name", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Icon))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter an icon", "OK");
                return;
            }

            if (MonthlyBudget <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Give a valid budget", "OK");
                return;
            }

            try
            {
                IsSaving = true;

                var newCategory = new Category
                {
                    UserId = UserId,
                    Name = Name,
                    Icon = Icon,
                    Color = Color,
                    MonthlyBudget = MonthlyBudget,
                    CreatedAt = DateTime.Now,
                    Type = Type
                };

                var created = await _categoryService.CreateCategoryAsync(newCategory);

                if (created != null)
                {
                    await Shell.Current.DisplayAlert("Success", "Category added!", "OK");
                    await Shell.Current.GoToAsync(nameof(CategoriesPage));
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to add category", "OK");
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
        void SetColor(string colorHex)
        {
            Color = colorHex;
        }

        [RelayCommand]
        async Task Cancel()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Cancel",
                "Discard this category?",
                "Yes",
                "No");

            if (confirm)
            {
                await Shell.Current.GoToAsync(nameof(CategoriesPage));
            }
        }

    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Views.Categories;
using Mde.Project.Mobile.Views.Dashboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private string? UserId => CurrentUser.GetUserId();

        public CategoriesViewModel(
            ICategoryService categoryService,
            ITransactionRecordService transactionService)
        {
            _categoryService = categoryService;
          
        }

        [ObservableProperty]
        private ObservableCollection<CategoryGroup> groupedCategories = new();

        [RelayCommand]
        async Task LoadCategories()
        {
            try
            {
                var cats = await _categoryService.GetAllCategoriesAsync(UserId);

                GroupedCategories.Clear();

                var expenseCategories = cats.Where(c => c.Type == "Expense").ToList();
                var incomeCategories = cats.Where(c => c.Type == "Income").ToList();

                if (incomeCategories.Any())
                {
                    GroupedCategories.Add(new CategoryGroup("INCOME", incomeCategories));
                }

                if (expenseCategories.Any())
                {
                    GroupedCategories.Add(new CategoryGroup("EXPENSE", expenseCategories));
                }

             
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task CategoryTapped(Category category)
        {
            await Shell.Current.GoToAsync(nameof(CategoryDetailPage), true,
                new Dictionary<string, object>
                {
                    { "CategoryId", category.Id }
                });
        }

        [RelayCommand]
        async Task AddCategory()
        {
            await Shell.Current.GoToAsync(nameof(CategoryAddPage));
        }

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("///home");
        }

    }
}

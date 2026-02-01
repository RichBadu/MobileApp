
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDatabaseService _databaseService;

        public CategoryService(IDatabaseService databaseService)
        {
          _databaseService = databaseService;
        }


        public async Task<Category> CreateCategoryAsync(Category category)
        {
            var db = await _databaseService.GetDatabaseAsync();

            if (string.IsNullOrEmpty(category.Id))
            {
                category.Id = Guid.NewGuid().ToString();
            }
            category.CreatedAt = DateTime.Now;
           
            await db.InsertAsync(category);
            return category;
        }

        public async Task CreateDefaultCategoriesAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            
            var existingCount = await db.Table<Category>()
                .Where(c => c.UserId == userId)
                .CountAsync();

            if (existingCount > 0)
                return;
            var categories = new List<Category>
    {
        // Expense Categories
           new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Other",
            Icon = "📦",
            Color = "#4CAF51",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Groceries",
            Icon = "🛒",
            Color = "#4CAF50",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Transport",
            Icon = "🚗",
            Color = "#2196F3",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Entertainment",
            Icon = "🎬",
            Color = "#FF9800",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Bills",
            Icon = "💡",
            Color = "#F44336",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Health",
            Icon = "⚕️",
            Color = "#E91E63",
            Type = "Expense",
            CreatedAt = DateTime.Now
        },
        
        // Income Categories
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Salary",
            Icon = "💰",
            Color = "#4CAF50",
            Type = "Income",
            CreatedAt = DateTime.Now
        },
        new Category
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = "Freelance",
            Icon = "💼",
            Color = "#00BCD4",
            Type = "Income",
            CreatedAt = DateTime.Now
        }
    };
            await db.InsertAllAsync(categories);
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var category = await GetCategoryByIdAsync(categoryId);

            if (category == null)
                return false;

            await db.DeleteAsync(category);
            return true;
        }


        public async Task<List<Category>> GetAllCategoriesAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();
           
            return await db.Table<Category>()
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        public async Task<Category> GetCategoryByName(string userId, string name)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<Category>()
                .Where(c => c.UserId == userId && c.Name == name)
                .OrderBy(c => c.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetCategoriesByTypeAsync(string userId, string type)
        {
            var db = await _databaseService.GetDatabaseAsync();
            
            return await db.Table<Category>()
                .Where(c => c.UserId == userId && c.Type == type)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<Category>()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
        }


        public async Task<decimal> GetTotalMonthlyBudgetAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            var categories = await db.Table<Category>()
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var totalBudget = categories.Sum(c => c.MonthlyBudget ?? 0m);
            return totalBudget;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var existing = await GetCategoryByIdAsync(category.Id);

            if (existing == null)
                throw new ArgumentException("Category not found");

            existing.Name = category.Name;
            existing.Icon = category.Icon;
            existing.Color = category.Color;
            existing.Type = category.Type;
            existing.MonthlyBudget = category.MonthlyBudget;

            await db.UpdateAsync(existing);
            return true;
        }

    }
}

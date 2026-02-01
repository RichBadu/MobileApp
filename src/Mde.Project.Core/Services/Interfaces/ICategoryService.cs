using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface ICategoryService
    {
        //Create
        Task<Category> CreateCategoryAsync(Category category);
        Task CreateDefaultCategoriesAsync(string userId);

        //Read
        Task<Category?> GetCategoryByIdAsync(string id);
        Task<Category> GetCategoryByName(string userId, string name);
        Task<List<Category>> GetAllCategoriesAsync(string userId);
        Task<List<Category>> GetCategoriesByTypeAsync(string userId, string type);
        Task<decimal> GetTotalMonthlyBudgetAsync(string userId);     
        //update
        Task<bool> UpdateCategoryAsync(Category category);
        //delete
        Task<bool> DeleteCategoryAsync(string categoryId);    

    }
}

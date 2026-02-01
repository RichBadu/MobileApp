using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Moq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Tests.Services
{
    public class CategoryServiceTests 
    {
        private SQLiteAsyncConnection _testDb;
        private Mock<IDatabaseService> _mockDatabaseService;
        private CategoryService _categoryService;

       
        private async Task SetupAsync()
        {
            _testDb = new SQLiteAsyncConnection(":memory:");
            await _testDb.CreateTableAsync<Category>();

            _mockDatabaseService = new Mock<IDatabaseService>();
            _mockDatabaseService
                .Setup(m => m.GetDatabaseAsync())
                .ReturnsAsync(_testDb);

            _categoryService = new CategoryService(_mockDatabaseService.Object);
        }


        [Fact]
        public async Task CategoryService_CreateCategory_GeneratesGuid()
        {
            await SetupAsync();
            //arrange
            var test = new Category
            {
                Name = "test",
                UserId = "user123",
                Type = "Expense"
            };
  
            //act
           var result =  await _categoryService.CreateCategoryAsync(test);

            //assert
            Assert.NotEqual(Guid.Empty.ToString(),result.Id);


            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task CategoryService_CreateCategory_SetsCreatedAtToNow()
        {
            await SetupAsync();
            //arrange
            var before = DateTime.Now;

            var test = new Category
            {
                Name = "test",
                UserId = "123",
                Type = "Expense"
            };

            //act
            var result = await _categoryService.CreateCategoryAsync(test);
            var after = DateTime.Now;

            //assert
            Assert.InRange(result.CreatedAt, before, after);


            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task CategoryService_CreateDefaultCategories_NewUser_Creates8Categories()
        {
            await SetupAsync();
            //arrange
            var userId = "test123";
            //act
            await _categoryService.CreateDefaultCategoriesAsync(userId);
            await _categoryService.CreateDefaultCategoriesAsync(userId);
            //assert
            var categories = await _testDb.Table<Category>()
                          .Where(c => c.UserId == userId)
                          .ToListAsync();

            Assert.Equal(8, categories.Count);

            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task CategoryService_CreateDefaultCategoriesTwice_CreatesOnly8Categories()
        {
            await SetupAsync();
            //arrange
            var userId = "test123";
            //act
            await _categoryService.CreateDefaultCategoriesAsync(userId);
            await _categoryService.CreateDefaultCategoriesAsync(userId);
            //assert
            var categories = await _testDb.Table<Category>()
                          .Where(c => c.UserId == userId)
                          .ToListAsync();

            Assert.Equal(8, categories.Count);

            await _testDb.CloseAsync();
        }

        [Theory]
        [InlineData(100,50,50,200)]
        [InlineData(100,0,100,200)]
        [InlineData(null,50,50,100)]
        public async Task GetTotalMonthlyBudget_ThreeCategories_ReturnsSumOfBudgets(decimal budget1,decimal budget2, decimal budget3, decimal expectedSum)
        {
            await SetupAsync();

            // Arrange
            var userId = "test-user";

            var categories = new List<Category>
            {
                new Category { Id = "1", UserId = userId, Name = "Food", MonthlyBudget = budget1 },
                new Category { Id = "2", UserId = userId, Name = "Transport", MonthlyBudget = budget2 },
                new Category { Id = "3", UserId = userId, Name = "Entertainment", MonthlyBudget = budget3 } 
            };

            await _testDb.InsertAllAsync(categories);

            // Act
            var result = await _categoryService.GetTotalMonthlyBudgetAsync(userId);

            // Assert
            Assert.Equal(expectedSum, result); 

            await _testDb.CloseAsync();
        }

        [Theory]
        [InlineData("Expense", "Expense","Income","Expense",2)]
        [InlineData("Income","Income", "Expense", "Expense",1)]
        [InlineData("Expense", "Expense", "Expense", "Income", 0)]
        [InlineData("Income", "Income", "Income", "Income", 3)]
        public async Task GetCategoriesByTypeAsync_GivenType_ReturnsOnlyGivenType(string type1,string type2, string type3,string givenType, int expectedCount)
        {
            await SetupAsync();

            // Arrange
            var userId = "test-user";
            var categories = new List<Category>
            {
                new Category { Id = "1", UserId = userId, Name = "Food", Type = type1 },
                new Category { Id = "2", UserId = userId, Name = "Salary", Type = type2 },
                new Category { Id = "3", UserId = userId, Name = "Transport", Type = type3 }
            };

            await _testDb.InsertAllAsync(categories);

            // Act
            var result = await _categoryService.GetCategoriesByTypeAsync(userId, givenType);

            // Assert
            Assert.Equal(expectedCount, result.Count);

            await _testDb.CloseAsync();
        }

    }
}

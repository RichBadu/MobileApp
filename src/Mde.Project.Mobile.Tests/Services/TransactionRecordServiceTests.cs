using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Moq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Mde.Project.Mobile.Tests.Services
{
    public class TransactionRecordServiceTests
    {
        private SQLiteAsyncConnection _testDb;
        private Mock<IDatabaseService> _mockDatabaseService;
        private TransactionRecordService _transactionRecordService;

        private async Task SetupAsync()
        {
            _testDb = new SQLiteAsyncConnection(":memory:");
            await _testDb.CreateTableAsync<TransactionRecord>();

            _mockDatabaseService = new Mock<IDatabaseService>();
            _mockDatabaseService
                .Setup(m => m.GetDatabaseAsync())
                .ReturnsAsync(_testDb);

            _transactionRecordService = new TransactionRecordService(_mockDatabaseService.Object);
        }
        [Fact]
        public async Task TransactionRecordService_CreateTransaction_GeneratesGuid()
        {
            await SetupAsync();
            //arrange
            var testCat = new Category
            {
                Id = "category",
                Name = "bakery"
            };

            var testTransaction = new TransactionRecord
            {
                Name = "bread",
                Amount = 14,
                CategoryId = "category"
            };
            //Act
          var result = await _transactionRecordService.CreateTransactionRecordAsync(testTransaction);
            //asssert
            Assert.NotEqual(Guid.Empty.ToString(), result.Id);

            await _testDb.CloseAsync();
        }



        [Theory]
        [InlineData(1,2025,1,2025,1,2025,3)]
        [InlineData(1, 2022, 1, 2025, 9, 2025,1)]
        [InlineData(6, 2025, 1, 2022, 7, 2025, 0)]

        public async Task TransactionRecordService_GetTransactionsByMonthAsync_ReturnsTranasctions(int month1, int year1,int month2,int year2, int month3, int year3, int expectedCount)
        {
            await SetupAsync();
            //arrange
            int fixedMonth = 1;
            int fixedYear = 2025;
            var testCat = new Category
            {
                Id = "testCat",
                Name = "test"
            };
            var mockedCatService = new Mock<ICategoryService>();
            mockedCatService.Setup(m => m.GetCategoryByName(It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(testCat);
           
            var userId = "tester123";

            List<TransactionRecord> testTransactions = new List<TransactionRecord>
            {
                new TransactionRecord
                {
                    Id = "testTrans1",
                    Name = "phone",
                    CategoryId = testCat.Id,
                    Date = new DateTime(year1,month1,1),
                    UserId = userId,

                },
                  new TransactionRecord
                {
                    Id = "testTrans2",
                    Name = "pool",
                    CategoryId = testCat.Id,
                    Date = new DateTime(year2,month2,1),
                    UserId = userId,

                },
                    new TransactionRecord
                {
                    Id = "testTrans3",
                    Name = "pen",
                    CategoryId = testCat.Id,
                    Date = new DateTime(year3,month3,1),
                    UserId = userId,
                },

            };
            await _testDb.InsertAllAsync(testTransactions);

            //Act
             var result = await _transactionRecordService.GetTransactionsByMonthAsync(userId,fixedMonth,fixedYear);
            //assert
            Assert.Equal(result.Count,expectedCount);

            await _testDb.CloseAsync();
        }



        [Theory]
        [InlineData("Expense",10,"Expense",5,15)]
        [InlineData("Income", 10, "Income", 5,0)]
        [InlineData("Expense", 10, "Income", 5,10)]

        public async Task TransactionRecordService_GetCategoryByMonthlyTotalAsync_ReturnsCorrectSum(string typeId1, int amount1, string typeId2, int amount2, int expected)
        {
            await SetupAsync();
            //arrange
            var userId = "tester";
            var categoryId = "food";
            var month = 1;
            var year = 2025;
            var transactionlist = new List<TransactionRecord>
        {
            new TransactionRecord { Id = "test1",UserId = userId,CategoryId =categoryId, Type = typeId1, Amount = amount1, Date = new DateTime(year,month,1)},
            new TransactionRecord { Id = "test2",UserId = userId,CategoryId = categoryId, Type = typeId2, Amount = amount2, Date = new DateTime(year,month,1) },
        };

           await _testDb.InsertAllAsync(transactionlist);

            //act
            var result = await _transactionRecordService.GetCategoryMonthlyTotalAsync(userId, categoryId, month, year);
            //assert
            Assert.Equal(result, expected);
            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task TransactionRecordService_ReassignTransaction_ReasignesCategoryCorrect()
        {
            await SetupAsync();
            //arrange
            var oldCatId = "oldCat";
            var userId = "tester";
            var newCatId = "newCat";
            var oldCategory = new Category
            {
                Id = oldCatId,
                UserId = userId,
            };

            var newCategory = new Category
            {
                Id = newCatId,
                UserId = userId,
            };

            var transaction = new TransactionRecord { Id = "test1",CategoryId = oldCatId, UserId = userId, };

            await _testDb.InsertAsync(transaction);
            //Act
            var result =  await _transactionRecordService.ReassignTransactionsAsync(oldCatId, newCatId);
            //assert

            var updatedTransaction = await _testDb.Table<TransactionRecord>()
                .Where(t => t.Id == "test1")
                .FirstOrDefaultAsync();

            Assert.Equal(updatedTransaction.CategoryId, newCategory.Id);
            Assert.True(result);
           await _testDb.CloseAsync();
        }

        [Fact]
        public async Task GetBalance_IncomeAndExpenses_ReturnsCorrectBalance()
        {
            await SetupAsync();

            var userId = "test-user";
            var month = 1;
            var year = 2025;
            decimal expectedBalance = 1000;
            var transactions = new List<TransactionRecord>
            {
                new TransactionRecord { Id = "1", UserId = userId, Type = "Income", Amount = 1000m, Date = new DateTime(year, month, 15), CategoryId = "salary" },
                new TransactionRecord { Id = "2", UserId = userId, Type = "Income", Amount = 500m, Date = new DateTime(year, month, 20), CategoryId = "bonus" },
                new TransactionRecord { Id = "3", UserId = userId, Type = "Expense", Amount = 200m, Date = new DateTime(year, month, 5), CategoryId = "food" },
                new TransactionRecord { Id = "4", UserId = userId, Type = "Expense", Amount = 300m, Date = new DateTime(year, month, 10), CategoryId = "transport" }
            };

            await _testDb.InsertAllAsync(transactions);

            //act
            var result = await _transactionRecordService.GetBalanceAsync(userId, month, year);

            //assert
            Assert.Equal(expectedBalance, result); 

        }
    }
}

using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Moq;
using SQLite;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Tests.Services
{
    public class RecurringTransactionRecordServiceTests
    {
        private SQLiteAsyncConnection _testDb;
        private Mock<IDatabaseService> _mockDatabaseService;
        private RecurringTransactionService _recurringTransactionService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<ITransactionRecordService> _transactionRecordService;

        private async Task SetupAsync()
        {
            _testDb = new SQLiteAsyncConnection(":memory:");
            await _testDb.CreateTableAsync<RecurringTransaction>();
            await _testDb.CreateTableAsync<Category>();
            await _testDb.CreateTableAsync<TransactionRecord>();
            _mockDatabaseService = new Mock<IDatabaseService>();
            _mockDatabaseService
                .Setup(m => m.GetDatabaseAsync())
                .ReturnsAsync(_testDb);

            _transactionRecordService = new Mock<ITransactionRecordService>();

            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService
                .Setup(m => m.ScheduleRecurringDueNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            _recurringTransactionService = new RecurringTransactionService(_transactionRecordService.Object, _mockDatabaseService.Object, _mockNotificationService.Object);
        }

        [Fact]

        public async Task GetDueRecurringTransactionsAsync_DailyFrequency_ReturnsDueTransactions()
        {
            await SetupAsync();

            //arrange
            var userId = "test";
            var today = DateTime.Now;
            var catId = "cat";

            var recurringTransactions = new List<RecurringTransaction>
            {
                new RecurringTransaction{ UserId = userId,Id = "test1",Name = "dailyTest1", Frequency = "Daily", IsActive = true, NextDueDate = today,CategoryId = catId },
                new RecurringTransaction{UserId = userId,Id = "test2",Name = "dailyTest2", Frequency = "Daily", IsActive = true, NextDueDate = today.AddDays(2), CategoryId = catId},

            };

            await _testDb.InsertAllAsync(recurringTransactions);

            //act
            var result = await _recurringTransactionService.GetDueRecurringTransactionsAsync(userId);
            //assert
            Assert.Single(result);


            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task GetDueRecurringTransactionsAsync_InactiveTransaction_NotReturned()
        {
            await SetupAsync();
            var userId = "test";
            var today = DateTime.Now;

            var inactiveRecurring = new RecurringTransaction
            {
                UserId = userId,
                Id = "inactive-test",
                Name = "Inactive Bill",
                Frequency = "Monthly",
                IsActive = false, 
                NextDueDate = today,
                CategoryId = "cat"
            };

            await _testDb.InsertAsync(inactiveRecurring);

            //act
            var result = await _recurringTransactionService.GetDueRecurringTransactionsAsync(userId);

            //assert
            Assert.Empty(result);

            await _testDb.CloseAsync();

        }




        [Fact]
        public async Task ProcessDueRecurringTransactionsAsync_WithAmountExceedingBudget_BugetWarningTriggered()
        {
            await SetupAsync();
            //arrange
            var userId = "test";
            var categoryId = "categoryTest";
            var transactionId = "transactionTest";
            var recurringId = "recurringTest";
            var today = DateTime.Now;
          
            var category = new Category { UserId = userId, Id = categoryId, Name = "test", Type = "Expense", MonthlyBudget = 100};
            var recurringTransaction = new RecurringTransaction { UserId = userId, Id = recurringId, CategoryId = categoryId, Name = "Monthly rent", Frequency = "Monthly", IsActive = true, NextDueDate = today, Amount = 200, Type = "Expense" };
            var transaction = new TransactionRecord { UserId = userId, Id = transactionId, CategoryId = categoryId, Amount = 200,Type = "Expense", Date = today};
          
            await _testDb.InsertAsync(category);
            await _testDb.InsertAsync(recurringTransaction);
            await _testDb.InsertAsync(transaction);

            _transactionRecordService.Setup( m => m.CreateTransactionRecordAsync(It.IsAny<TransactionRecord>()))
                .ReturnsAsync(transaction);
            //Act
             var result  = await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(userId);
            //assert
            _mockNotificationService.Verify(m => m.ShowBudgetWarningAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Once);
            Assert.True(result > 0);
            await _testDb.CloseAsync(); 
        }

        [Fact]
        public async Task ProcessDueRecurringTransactionsAsync_WithCorrectInput_CalculatesCorrectNextDueDateForMonth()
        {
            await SetupAsync();
            //arrange
            var userId = "test";
            var categoryId = "categoryTest";
            var transactionId = "transactionTest";
            var recurringId = "recurringTest";
            var today = DateTime.Now;
            var expected = today.AddMonths(1);
            var category = new Category { UserId = userId, Id = categoryId, Name = "test", Type = "Expense", MonthlyBudget = 100 };
            var recurringTransaction = new RecurringTransaction { UserId = userId, Id = recurringId, CategoryId = categoryId, Name = "Monthly rent", Frequency = "Monthly", IsActive = true, NextDueDate = today, Amount = 200, Type = "Expense" };
            var transaction = new TransactionRecord { UserId = userId, Id = transactionId, CategoryId = categoryId, Amount = 200, Type = "Expense", Date = today };

            await _testDb.InsertAsync(category);
            await _testDb.InsertAsync(recurringTransaction);
            await _testDb.InsertAsync(transaction);

            _transactionRecordService.Setup(m => m.CreateTransactionRecordAsync(It.IsAny<TransactionRecord>()))
            .ReturnsAsync(transaction);

            //act
            var result =  await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(userId);
            var updated = await _testDb.Table<RecurringTransaction>()
            .Where(r => r.Id == recurringId)
            .FirstOrDefaultAsync();
            //assert
            Assert.Equal(expected.Month, updated.NextDueDate.Value.Month);
        }



        [Fact]
        public async Task ProcessDueRecurringTransactionsAsync_WithCorrectInput_CalculatesCorrectNextDueDateForDay()
        {
            await SetupAsync();
            //arrange
            var userId = "test";
            var categoryId = "categoryTest";
            var transactionId = "transactionTest";
            var recurringId = "recurringTest";
            var today = DateTime.Now;
            var expected = today.AddDays(1);
            var category = new Category { UserId = userId, Id = categoryId, Name = "test", Type = "Expense", MonthlyBudget = 100 };
            var recurringTransaction = new RecurringTransaction { UserId = userId, Id = recurringId, CategoryId = categoryId, Name = "daily rent", Frequency = "Daily", IsActive = true, NextDueDate = today, Amount = 200, Type = "Expense" };
            var transaction = new TransactionRecord { UserId = userId, Id = transactionId, CategoryId = categoryId, Amount = 200, Type = "Expense", Date = today };

            await _testDb.InsertAsync(category);
            await _testDb.InsertAsync(recurringTransaction);
            await _testDb.InsertAsync(transaction);

            _transactionRecordService.Setup(m => m.CreateTransactionRecordAsync(It.IsAny<TransactionRecord>()))
            .ReturnsAsync(transaction);

            //act
            var result = await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(userId);
            var updated = await _testDb.Table<RecurringTransaction>()
            .Where(r => r.Id == recurringId)
            .FirstOrDefaultAsync();
            //assert
            Assert.Equal(expected.Day, updated.NextDueDate.Value.Day);
        }

        [Fact]
        public async Task ProcessDueRecurringTransactionsAsync_WithCorrectInput_CalculatesCorrectNextDueDateForWeek()
        {
            await SetupAsync();
            //arrange
            var userId = "test";
            var categoryId = "categoryTest";
            var transactionId = "transactionTest";
            var recurringId = "recurringTest";
            var today = DateTime.Now;
            var expected = today.AddDays(7);
            var dayOfweek = (int)today.DayOfWeek;
            var category = new Category { UserId = userId, Id = categoryId, Name = "test", Type = "Expense", MonthlyBudget = 100 };
            var recurringTransaction = new RecurringTransaction { UserId = userId, Id = recurringId, CategoryId = categoryId, Name = "weekly rent", Frequency = "Weekly", IsActive = true, NextDueDate = today, Amount = 200, Type = "Expense", DayOfWeek = dayOfweek};
            var transaction = new TransactionRecord { UserId = userId, Id = transactionId, CategoryId = categoryId, Amount = 200, Type = "Expense", Date = today };

            await _testDb.InsertAsync(category);
            await _testDb.InsertAsync(recurringTransaction);
            await _testDb.InsertAsync(transaction);

            _transactionRecordService.Setup(m => m.CreateTransactionRecordAsync(It.IsAny<TransactionRecord>()))
            .ReturnsAsync(transaction);

            //act
            var result = await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(userId);
            var updated = await _testDb.Table<RecurringTransaction>()
            .Where(r => r.Id == recurringId)
            .FirstOrDefaultAsync();
         
            //assert
            Assert.Equal(expected.Day, updated.NextDueDate.Value.Day);
        }

        [Fact]
        public async Task ProcessDueRecurringTransactionsAsync_WithCorrectInput_CalculatesCorrectNextDueDateForYear()
        {
            await SetupAsync();
            //arrange
            var userId = "test";
            var categoryId = "categoryTest";
            var transactionId = "transactionTest";
            var recurringId = "recurringTest";
            var today = DateTime.Now;
            var expected = today.AddYears(1);
            var category = new Category { UserId = userId, Id = categoryId, Name = "test", Type = "Expense", MonthlyBudget = 100 };
            var recurringTransaction = new RecurringTransaction { UserId = userId, Id = recurringId, CategoryId = categoryId, Name = "weekly rent", Frequency = "Yearly", IsActive = true, NextDueDate = today, Amount = 200, Type = "Expense"};
            var transaction = new TransactionRecord { UserId = userId, Id = transactionId, CategoryId = categoryId, Amount = 200, Type = "Expense", Date = today };

            await _testDb.InsertAsync(category);
            await _testDb.InsertAsync(recurringTransaction);
            await _testDb.InsertAsync(transaction);

            _transactionRecordService.Setup(m => m.CreateTransactionRecordAsync(It.IsAny<TransactionRecord>()))
            .ReturnsAsync(transaction);

            //act
            var result = await _recurringTransactionService.ProcessDueRecurringTransactionsAsync(userId);
            var updated = await _testDb.Table<RecurringTransaction>()
            .Where(r => r.Id == recurringId)
            .FirstOrDefaultAsync();

            //assert
            Assert.Equal(expected.Year, updated.NextDueDate.Value.Year);
        }

    }
}

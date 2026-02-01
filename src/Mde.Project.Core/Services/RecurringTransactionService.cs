using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {

        private readonly IDatabaseService _databaseService;
        private readonly ITransactionRecordService _transactionRecordService;
        private readonly INotificationService _notificationService;
        public RecurringTransactionService(ITransactionRecordService transactionRecordService, IDatabaseService databaseService, INotificationService notificationService)
        {
            _databaseService = databaseService;
            _transactionRecordService = transactionRecordService;
            _notificationService = notificationService;
        }
        public async Task<RecurringTransaction> CreateRecurringTransactionAsync(RecurringTransaction recurring)
        {
            var db = await _databaseService.GetDatabaseAsync();
            if (string.IsNullOrEmpty(recurring.Id))
            {
                recurring.Id = Guid.NewGuid().ToString();

            }

            recurring.CreatedAt = DateTime.Now;
            recurring.IsActive = true;

            recurring.NextDueDate = CalculateNextDueDate(recurring);

            await db.InsertAsync(recurring);

                await _notificationService.ScheduleRecurringDueNotificationAsync(
                recurring.Id,
                recurring.Name,
                recurring.Amount,
                (DateTime)recurring.NextDueDate);
            return recurring;
       }

        public async Task<bool> DeleteRecurringTransactionAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var recurring = await GetRecurringTransactionByIdAsync(id);

            if (recurring == null)
                return false;

           
            await _notificationService.CancelRecurringDueNotificationAsync(recurring.Id);

            await db.DeleteAsync(recurring);
            return true;
        }
        public async Task<List<RecurringTransaction>> GetActiveRecurringTransactionsAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<RecurringTransaction>()
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderBy(r => r.NextDueDate)
                .ToListAsync();
        }
        public async Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<RecurringTransaction>()
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.NextDueDate)
                .ToListAsync();
        }

        public async Task<List<RecurringTransaction>> GetDueRecurringTransactionsAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            var today = DateTime.Today;

            var allActive = await db.Table<RecurringTransaction>()
                .Where(r => r.UserId == userId && r.IsActive)
                .ToListAsync();

            
            return allActive
                .Where(r => r.NextDueDate.HasValue && r.NextDueDate.Value.Date <= today)
                .ToList();
        }

        public async Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<RecurringTransaction>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> ProcessDueRecurringTransactionsAsync(string userId)
        {

            var dueTransactions = await GetDueRecurringTransactionsAsync(userId);
            int processedCount = 0;

            foreach (var recurring in dueTransactions)
            {
                try
                {
                  
                    var transaction = new TransactionRecord
                    {
                        UserId = recurring.UserId,
                        Name = recurring.Name,
                        CategoryId = recurring.CategoryId,
                        Amount = recurring.Amount,
                        Type = recurring.Type,
                        Date = DateTime.Now,
                        Description = $"{recurring.Name} (Recurring)"
                    };

                    await _transactionRecordService.CreateTransactionRecordAsync(transaction);


                    if (transaction.Type == "Expense")
                    {
                        await CheckBudgetWarningAsync(transaction);
                    }

                    recurring.LastProcessedDate = DateTime.Now;
                    recurring.NextDueDate = CalculateNextDueDate(recurring);

                    var db = await _databaseService.GetDatabaseAsync();
                    await db.UpdateAsync(recurring);
                    await _notificationService.ScheduleRecurringDueNotificationAsync(
                          recurring.Id,
                          recurring.Name,
                          recurring.Amount,
                         (DateTime)recurring.NextDueDate);

                    processedCount++;
                }
                catch (Exception ex)
                {
                   
                    System.Diagnostics.Debug.WriteLine($"Error processing recurring transaction {recurring.Id}: {ex.Message}");
                }
            }

            return processedCount;

        }
        public async Task<RecurringTransaction> UpdateRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            var db = await _databaseService.GetDatabaseAsync();

            await db.UpdateAsync(recurringTransaction);

            return recurringTransaction;
        }

        public async Task<bool> ToggleRecurringActiveAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var recurring = await GetRecurringTransactionByIdAsync(id);

            if (recurring == null)
                return false;

            recurring.IsActive = !recurring.IsActive;
            await db.UpdateAsync(recurring);

            if (recurring.IsActive)
            {
                
                await _notificationService.ScheduleRecurringDueNotificationAsync(
                    recurring.Id,
                    recurring.Name,
                    recurring.Amount,
                    (DateTime)recurring.NextDueDate
                );
            }
            else
            {
               
                await _notificationService.CancelRecurringDueNotificationAsync(recurring.Id);
            }

            return true;
        }

        private DateTime CalculateNextDueDate(RecurringTransaction recurring)
        {
            var baseDate = recurring.LastProcessedDate?.Date ?? DateTime.Today;


            return recurring.Frequency switch
            {
                "Daily" => baseDate.AddDays(1),
                "Weekly" => GetNextWeekday(baseDate, recurring.DayOfWeek ?? 1),
                "Monthly" => GetNextMonthlyDate(baseDate, recurring.DayOfMonth ?? 1),
                "Yearly" => GetNextYearlyDate(baseDate, recurring.DayOfMonth ?? 1),
                _ => baseDate.AddDays(1)
            };
        }
        private DateTime GetNextWeekday(DateTime baseDate, int dayOfWeek)
        {
         
            int daysUntilTarget = ((dayOfWeek - (int)baseDate.DayOfWeek + 7) % 7);

            if (daysUntilTarget == 0)
                daysUntilTarget = 7; 

            return baseDate.AddDays(daysUntilTarget);
        }

        private DateTime GetNextMonthlyDate(DateTime baseDate,int day)
        {
           

            int validDay = Math.Min(day, DateTime.DaysInMonth(baseDate.Year, baseDate.Month));
            var nextDate = new DateTime(baseDate.Year, baseDate.Month, validDay);

            if (nextDate <= baseDate)
            {
                var nextMonth = baseDate.AddMonths(1);
                validDay = Math.Min(day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                nextDate = new DateTime(nextMonth.Year, nextMonth.Month, validDay);
            }

            return nextDate;
        }
        private DateTime GetNextYearlyDate(DateTime baseDate, int day)
        {
           
            int validDay = Math.Min(day, DateTime.DaysInMonth(baseDate.Year, baseDate.Month));
            var nextDate = new DateTime(baseDate.Year, baseDate.Month, validDay);

            if (nextDate <= baseDate)
            {
                var nextYear = baseDate.AddYears(1);
                validDay = Math.Min(day, DateTime.DaysInMonth(nextYear.Year, nextYear.Month));
                nextDate = new DateTime(nextYear.Year, nextYear.Month, validDay);
            }

            return nextDate;
        }

        private async Task CheckBudgetWarningAsync(TransactionRecord transaction)
        {

            var db = await _databaseService.GetDatabaseAsync();
            var category = await db.Table<Category>()
                .Where(c => c.Id == transaction.CategoryId)
                .FirstOrDefaultAsync();


            if (category?.MonthlyBudget == null || category.MonthlyBudget <= 0)
                return;

            var allTransactions = await db.Table<TransactionRecord>()
                                          .Where(t => t.UserId == transaction.UserId)
                                          .ToListAsync();

            var totalSpent = allTransactions
                   .Where(t => t.CategoryId == category.Id
                            && t.Type == "Expense"
                            && t.Date.Month == DateTime.Now.Month
                            && t.Date.Year == DateTime.Now.Year)
                   .Sum(t => t.Amount);


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

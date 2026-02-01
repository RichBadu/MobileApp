using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Mde.Project.Mobile.Core.Services
{
    public class TransactionRecordService : ITransactionRecordService
    {

        private readonly IDatabaseService _databaseService;

        public TransactionRecordService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

      

        public async Task<TransactionRecord> CreateTransactionRecordAsync(TransactionRecord transaction)
        {
            var db = await _databaseService.GetDatabaseAsync();
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();

            }
            transaction.CreatedAt = DateTime.Now;

            await db.InsertAsync(transaction);
            return transaction;
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var transaction = await GetTransactionByIdAsync(id);

            if (transaction == null)
                return false;

            await db.DeleteAsync(transaction);
            return true;
        }

        public async Task<bool> DeleteTransactionsByCategoryAsync(string categoryId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            var transactionsToDelete = await db.Table<TransactionRecord>()
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();

            if (transactionsToDelete.Count == 0)
                return false;

            foreach (var transaction in transactionsToDelete)
            {
                await db.DeleteAsync(transaction);
            }

            return true;
        }

        public async Task<List<TransactionRecord>> GetAllTransactionRecordsAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<TransactionRecord>()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetBalanceAsync(string userId, int month, int year)
        {
            var transactions = await GetTransactionsByMonthAsync(userId, month, year);

            var income = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var expenses = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            return income - expenses;
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, int month, int year)
        {
            var transactions = await GetTransactionsByMonthAsync(userId, month, year);

            return transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.CategoryId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Amount)
                );
        }

        public async Task<List<TransactionRecord>> GetRecentTransactionsAsync(string userId, int count = 5)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<TransactionRecord>()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalExpensesAsync(string userId, int month, int year)
        {
            var transactions = await GetTransactionsByMonthAsync(userId, month, year);

            return transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);
        }

        public async Task<decimal> GetTotalIncomeAsync(string userId, int month, int year)
        {
            var transactions = await GetTransactionsByMonthAsync(userId, month, year);

            return transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);
        }

        public async Task<List<TransactionRecord>> GetTransactionsByCategoryAsync(string userId, string categoryId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<TransactionRecord>()
                .Where(t => t.UserId == userId && t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<TransactionRecord?> GetTransactionByIdAsync(string id)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<TransactionRecord>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TransactionRecord>> GetTransactionByTypeAsync(string userId, string type)
        {
            var db = await _databaseService.GetDatabaseAsync();

            return await db.Table<TransactionRecord>()
                .Where(t => t.UserId == userId && t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<TransactionRecord>> GetTransactionsByMonthAsync(string userId, int month, int year)
        {
            var db = await _databaseService.GetDatabaseAsync();

            var allTransactions = await db.Table<TransactionRecord>()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            
            return allTransactions
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

   

        public async Task<TransactionRecord> UpdateTransactionAsync(TransactionRecord transaction)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var existing = await GetTransactionByIdAsync(transaction.Id);

            if (existing == null)
                throw new ArgumentException("Transaction not found");

            existing.CategoryId = transaction.CategoryId;
            existing.Amount = transaction.Amount;
            existing.Type = transaction.Type;
            existing.Date = transaction.Date;
            existing.Description = transaction.Description;
            existing.PhotoPath = transaction.PhotoPath;
            existing.UpdatedAt = DateTime.Now;

            await db.UpdateAsync(existing);
            return existing;
        }

        public async Task<bool> ReassignTransactionsAsync(string oldCategoryId, string newCategoryId)
        {
            var db = await _databaseService.GetDatabaseAsync();

            var transactionsToReassign = await db.Table<TransactionRecord>()
                .Where(t => t.CategoryId == oldCategoryId)
                .ToListAsync();

            if (transactionsToReassign.Count == 0)
                return false;

            foreach (var transaction in transactionsToReassign)
            {
                transaction.CategoryId = newCategoryId;
                transaction.UpdatedAt = DateTime.Now;
                await db.UpdateAsync(transaction);
            }

            return true;
        }
        public async Task<decimal> GetCategoryMonthlyTotalAsync(string userId, string categoryId, int month, int year)
        {
            var transactions = await GetTransactionsByMonthAsync(userId, month, year);

            return transactions
                .Where(t => t.CategoryId == categoryId && t.Type == "Expense")
                .Sum(t => t.Amount);
        }
    }
}

using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface ITransactionRecordService
    {
        //create
        Task<TransactionRecord> CreateTransactionRecordAsync(TransactionRecord transaction);

        //read
        Task<List<TransactionRecord>> GetAllTransactionRecordsAsync(string userId);
        Task<TransactionRecord?> GetTransactionByIdAsync(string id);
        Task<List<TransactionRecord>> GetTransactionsByCategoryAsync(string userId, string categoryId);
        Task<List<TransactionRecord>> GetTransactionByTypeAsync(string userId,string type);
        Task<List<TransactionRecord>> GetTransactionsByMonthAsync(string userId, int month, int year);
        Task<List<TransactionRecord>> GetRecentTransactionsAsync(string userId, int count = 5);
        Task<decimal> GetCategoryMonthlyTotalAsync(string userId, string categoryId, int month, int year);


        //update
        Task<TransactionRecord> UpdateTransactionAsync(TransactionRecord transaction);
        Task<bool> ReassignTransactionsAsync(string oldCategoryId, string newCategoryId);

        //delete
        Task<bool> DeleteTransactionAsync(string id);
        Task<bool> DeleteTransactionsByCategoryAsync(string categoryId);

        //stats
        Task<decimal> GetTotalIncomeAsync(string userId, int month, int year);
        Task<decimal> GetTotalExpensesAsync(string userId, int month, int year);
        Task<decimal> GetBalanceAsync(string userId, int month, int year);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, int month, int year);

    }
}

using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface IRecurringTransactionService
    {
        //create
        Task<RecurringTransaction> CreateRecurringTransactionAsync(RecurringTransaction recurring);

        //read
        Task<RecurringTransaction?> GetRecurringTransactionByIdAsync(string id);
        Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync(string userId);
        Task<List<RecurringTransaction>> GetActiveRecurringTransactionsAsync(string userId);
        Task<List<RecurringTransaction>> GetDueRecurringTransactionsAsync(string userId);

        //update
        Task<bool> ToggleRecurringActiveAsync(string id);
        Task<RecurringTransaction> UpdateRecurringTransactionAsync(RecurringTransaction recurring);
        //delete
        Task<bool> DeleteRecurringTransactionAsync(string id);

        //process
        Task<int> ProcessDueRecurringTransactionsAsync(string userId);             
    }
}

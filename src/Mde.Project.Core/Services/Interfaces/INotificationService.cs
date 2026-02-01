using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface INotificationService
    {
       
        Task ShowBudgetWarningAsync(string categoryName, decimal spent, decimal budget);


        Task<int> ScheduleRecurringDueNotificationAsync(string recurringId, string transactionName, decimal amount, DateTime dueDate);
        Task CancelRecurringDueNotificationAsync(string recurringId);

        Task ScheduleDailyReminderAsync();
        Task CancelDailyReminderAsync();    
        Task<bool> RequestPermissionsAsync();
        Task CancelAllNotificationsAsync();
    }
}

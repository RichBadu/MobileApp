using Mde.Project.Mobile.Core.Services.Interfaces;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.iOSOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INotificationService = Mde.Project.Mobile.Core.Services.Interfaces.INotificationService;


namespace Mde.Project.Mobile.Domain.Services
{
    public class NotificationService : INotificationService
    {
        private const int DAILY_REMINDER_ID = 1000;
        private const int RECURRING_BASE_ID = 3000;
        private int _nextDynamicId = 100;

        public async Task ShowBudgetWarningAsync(string categoryName, decimal spent, decimal budget)
        {
            var percentage = (spent / budget) * 100;
#if WINDOWS
    // 🪟 Windows: show dialog
    await Application.Current.MainPage.DisplayAlert(
        "⚠️ Budget Alert",
        $"{categoryName}: You've spent €{spent:N2} of €{budget:N2}",
        "OK"
    );
#else
            var notification = new NotificationRequest
            {
                NotificationId = _nextDynamicId++,
                Title = "🚨 Budget Alert!",
                Description = $"You've exceeded your {categoryName} budget\n€{spent:N2}/€{budget:N2} ({percentage:N0}% used)",
                BadgeNumber = 1,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                },
                Android = new AndroidOptions
                {
                    ChannelId = "budget_alerts",
                    Priority = AndroidPriority.High,
                    VibrationPattern = new long[] { 0, 500 }
                }              
            };

            await LocalNotificationCenter.Current.Show(notification);
#endif

        }
        public  Task CancelAllNotificationsAsync()
        {
             LocalNotificationCenter.Current.CancelAll();
   
            return Task.CompletedTask;
        }

        public async Task ScheduleDailyReminderAsync()
        {
            await CancelDailyReminderAsync();

            var notifyTime = DateTime.Today.AddHours(20);

            if (DateTime.Now >= notifyTime)
            {
                notifyTime = notifyTime.AddDays(1);
            }

            var notification = new NotificationRequest
            {
                NotificationId = DAILY_REMINDER_ID,
                Title = "💰 Don't forget!",
                Description = "Did you track your expenses today?\nOpen Trackeroo",
                BadgeNumber = 1,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime,
                    RepeatType = NotificationRepeat.Daily
                },
                Android = new AndroidOptions
                {
                    ChannelId = "daily_reminders",
                    Priority = AndroidPriority.Default,
                    LaunchAppWhenTapped = true
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
       

        }

        public Task CancelDailyReminderAsync()
        {
            LocalNotificationCenter.Current.Cancel(DAILY_REMINDER_ID);
            return Task.CompletedTask;
        }

    
        private int GetRecurringNotificationId(string recurringId)
        {
           
            return RECURRING_BASE_ID + Math.Abs(recurringId.GetHashCode() % 1000);
        }


        public async Task<int> ScheduleRecurringDueNotificationAsync(string recurringId, string transactionName, decimal amount, DateTime dueDate)
        {
            var notificationId = GetRecurringNotificationId(recurringId);

            var notification = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = "🔄 Payment Due",
                Description = $"{transactionName} (€{amount:N2}) is due today\nTap to view transaction",
                BadgeNumber = 1,

                ReturningData = recurringId,

                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = dueDate

                },

                Android = new AndroidOptions
                {
                    ChannelId = "recurring_transactions",
                    Priority = AndroidPriority.Default,

                    LaunchAppWhenTapped = true
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
            return notificationId;

        }
        public Task CancelRecurringDueNotificationAsync(string recurringId)
        {
            var notificationId = GetRecurringNotificationId(recurringId);
            LocalNotificationCenter.Current.Cancel(notificationId);
            return Task.CompletedTask;

        }
        public async Task<bool> RequestPermissionsAsync()
        {

            if(await LocalNotificationCenter.Current.AreNotificationsEnabled())
            {
                return true;
            }

           var result = await LocalNotificationCenter.Current.RequestNotificationPermission();

            return result;
        }
  

    } 
     
}

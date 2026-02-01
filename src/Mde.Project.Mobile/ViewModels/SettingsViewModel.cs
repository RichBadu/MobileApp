using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Models.SupaBase;
using Mde.Project.Mobile.Core.Services;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Domain.Services;
using Mde.Project.Mobile.Views.Categories;
using Mde.Project.Mobile.Views.RecurringTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ISyncService _syncService;
        private readonly INotificationService _notificationService;

     
        private bool _isUpdatingToggles = false;
        private string? UserId => CurrentUser.GetUserId();

        public SettingsViewModel(IUserService userService, ISyncService syncService, INotificationService notificationService)
        {
            _userService = userService;
            _syncService = syncService;
            _notificationService = notificationService;

          

            NotificationsEnabled = Preferences.Get("NotificationsEnabled", false);
            DailyReminderEnabled = Preferences.Get("DailyReminderEnabled", false);
            BudgetWarningEnabled = Preferences.Get("BudgetWarningEnabled", false);


            AutoSyncEnabled = _syncService.IsAutoSyncEnabled();
            LastSyncText = _syncService.GetLastSyncDisplayText();
        
        }

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string userEmail = string.Empty;

        [ObservableProperty]
        private bool notificationsEnabled;
    
        [ObservableProperty]
        private bool dailyReminderEnabled;

        [ObservableProperty]
        private bool budgetWarningEnabled;

        [ObservableProperty]
        private bool autoBackupEnabled;

        [ObservableProperty]
        private string lastBackupDate = "Never";

        [ObservableProperty]
        private bool autoSyncEnabled;
       
        [ObservableProperty]
        private string lastSyncText = "Never synced";

        [ObservableProperty]
        private bool isSyncing;

        [RelayCommand]
        async Task LoadUserData()
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(UserId);

                if (user != null)
                {
                    UserName = user.DisplayName;
                    UserEmail = user.Email;
                  
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load user data: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        async Task ViewRecurringTransactions()
        {
            await Shell.Current.GoToAsync(nameof(RecurringTransactionsPage));
        }

        [RelayCommand]
        async Task Logout()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Logout",
                "Are you sure you want to logout?",
                "Logout",
                "Cancel");

            if (confirm)
            {
              
                Preferences.Clear();
                CurrentUser.Clear();
              

                // Navigate to login
                await Shell.Current.GoToAsync("//login");
            }
        }

        [RelayCommand]
        async Task ManualSync()
        {
            if (IsSyncing) return;

            try
            {
                IsSyncing = true;

                var userId = CurrentUser.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    await Shell.Current.DisplayAlert("Error", "Not logged in", "OK");
                    return;
                }

                var (success, error) = await _syncService.SyncAllDataAsync(userId);

                if (success)
                {
                    LastSyncText = "Just now";
                    await Shell.Current.DisplayAlert("Success", "Sync completed!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", $"Sync failed: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Sync error: {ex.Message}", "OK");
            }
            finally
            {
                IsSyncing = false;
                LastSyncText = _syncService.GetLastSyncDisplayText();
            }
        }
 
        partial void OnNotificationsEnabledChanged(bool value)
        {
            if (_isUpdatingToggles) return;
            HandleNotificationToggle(value);
        }

     
        partial void OnDailyReminderEnabledChanged(bool value)
        {
            if (_isUpdatingToggles) return;
            HandleDailyReminderToggle(value);
        }
        async partial void OnBudgetWarningEnabledChanged(bool value)
        {
            if (_isUpdatingToggles) return; 

            if (!NotificationsEnabled)
            {
                // Reset toggle if notifications disabled
                _isUpdatingToggles = true;
                BudgetWarningEnabled = false;
                _isUpdatingToggles = false;

                await Toast.Make("Enable notifications first").Show();
                return;
            }

            Preferences.Set("BudgetWarningEnabled", value);

        }
        partial void OnAutoSyncEnabledChanged(bool value)
        {
            _syncService.SetAutoSyncEnabled(value);
            System.Diagnostics.Debug.WriteLine($"Auto-sync {(value ? "enabled" : "disabled")}");
        }

        private async void HandleNotificationToggle(bool value)
        {
        
            try
            {
                if (value)
                {
                    var granted = await _notificationService.RequestPermissionsAsync();
                    if (granted)
                    {
                        _isUpdatingToggles = true;
                        DailyReminderEnabled = true;
                        Preferences.Set("DailyReminderEnabled", true);
                        await _notificationService.ScheduleDailyReminderAsync();

                        BudgetWarningEnabled = true;
                        Preferences.Set("BudgetWarningEnabled", true);

                        _isUpdatingToggles = false;

                    }
                    else
                    {
                        _isUpdatingToggles = true;
                        NotificationsEnabled = false;
                        Preferences.Set("NotificationsEnabled", false);
                        _isUpdatingToggles = false;
                        await Shell.Current.DisplayAlert("Permission Denied", "Notification permissions were denied", "OK");
                    }
                }
                else
                {
                    await _notificationService.CancelAllNotificationsAsync();
                    _isUpdatingToggles = true;

                    DailyReminderEnabled = false;
                    Preferences.Set("DailyReminderEnabled", false);
                    System.Diagnostics.Debug.WriteLine($"Set DailyReminderEnabled to false");


                    BudgetWarningEnabled = false;
                    Preferences.Set("BudgetWarningEnabled", false);
                    System.Diagnostics.Debug.WriteLine($"Set BudgetWarningEnabled to false");

                    _isUpdatingToggles = false;

                }
            }
            catch (Exception ex)
            {
                _isUpdatingToggles = false;
                System.Diagnostics.Debug.WriteLine($"Notification toggle error: {ex.Message}");
            }
        }


        private async void HandleDailyReminderToggle(bool value)
        {
            Preferences.Set("DailyReminderEnabled", value);

            if (!NotificationsEnabled)
            {
                _isUpdatingToggles = true;
                DailyReminderEnabled = false;
                _isUpdatingToggles = false;
                await Toast.Make("Enable notifications first").Show();
                return;
            }
            Preferences.Set("DailyReminderEnabled", value);
            try
            {
                if (value)
                {
                    await _notificationService.ScheduleDailyReminderAsync();
                }
                else
                {
                    await _notificationService.CancelDailyReminderAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Daily reminder error: {ex.Message}");
            }
        }

    }
}
     
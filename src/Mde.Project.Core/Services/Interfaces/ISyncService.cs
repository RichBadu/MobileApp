using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface ISyncService
    {
      
        Task<(bool Success, string Error)> UploadAllDataAsync(string userId);
        Task<(bool Success, string Error)> UploadCategoriesAsync(string userId);
        Task<(bool Success, string Error)> UploadTransactionsAsync(string userId);
        Task<(bool Success, string Error)> UploadRecurringTransactionsAsync(string userId);
        Task<(bool Success, string Error)> UploadUserAsync(string userId);

        Task<(bool Success, string Error)> DownloadAllDataAsync(string userId);
        Task<(bool Success, string Error)> DownloadCategoriesAsync(string userId);
        Task<(bool Success, string Error)> DownloadTransactionsAsync(string userId);
        Task<(bool Success, string Error)> DownloadRecurringTransactionsAsync(string userId);
        Task<(bool Success, string Error)> DownloadUserAsync(string userId);


        Task<(bool Success, string Error)> SyncAllDataAsync(string userId);

        DateTime? GetLastSyncTime();
        bool IsAutoSyncEnabled();
        void SetAutoSyncEnabled(bool enabled);
        bool ShouldAutoSync();
        string GetLastSyncDisplayText();
    }
}

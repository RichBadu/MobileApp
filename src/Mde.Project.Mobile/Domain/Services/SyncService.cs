using Mde.Project.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Models.SupaBase;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using User = Mde.Project.Mobile.Core.Models.User;

namespace Mde.Project.Mobile.Domain.Services
{
    public class SyncService : ISyncService
    {
        private readonly ISupabaseAuthService _authService;
        private readonly ICategoryService _categoryService;
        private readonly ITransactionRecordService _transactionService;
        private readonly IRecurringTransactionService _recurringService;
        private readonly IUserService _userService;

        private const string LastSyncKey = "LastSyncTime";
        private const string AutoSyncEnabledKey = "AutoSyncEnabled";
        private bool _isSyncing = false;

        public SyncService(
            ISupabaseAuthService authService,
            ICategoryService categoryService,
            ITransactionRecordService transactionService,
            IRecurringTransactionService recurringService,
            IUserService userService)
        {
            _authService = authService;
            _categoryService = categoryService;
            _transactionService = transactionService;
            _recurringService = recurringService;
            _userService = userService;
        }

        public async Task<(bool Success, string Error)> UploadCategoriesAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

               
                var localCategories = await _categoryService.GetAllCategoriesAsync(userId);

                if (!localCategories.Any())
                    return (true, string.Empty); 
              
                await client
                     .From<SupaBaseCategory>()
                     .Where(x => x.UserId == userId)
                     .Delete();

                var supabaseCategories = localCategories.Select(lc => new SupaBaseCategory
                {
                    Id = lc.Id,
                    UserId = lc.UserId,
                    Name = lc.Name,
                    Icon = lc.Icon,
                    Color = lc.Color,
                    MonthlyBudget = lc.MonthlyBudget,
                    Type = lc.Type,
                    CreatedAt = lc.CreatedAt,
                }).ToList();

                
                await client.From<SupaBaseCategory>().Upsert(supabaseCategories);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Upload categories failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> UploadTransactionsAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

             
                var localTransactions = await _transactionService.GetAllTransactionRecordsAsync(userId);

                if (!localTransactions.Any())
                    return (true, string.Empty);

                await client
                     .From<SupabaseTransaction>()
                     .Where(x => x.UserId == userId)
                     .Delete();

                var supabaseTransactions = localTransactions.Select(lt => new SupabaseTransaction
                {
                    Id = lt.Id,
                    UserId = lt.UserId,
                    CategoryId = lt.CategoryId,
                    Name = lt.Name,
                    Amount = lt.Amount,
                    Type = lt.Type,
                    Description = lt.Description,
                    Date = lt.Date,
                    PhotoPath = lt.PhotoPath,
                    CreatedAt = lt.CreatedAt,
                    UpdatedAt = lt.UpdatedAt
                }).ToList();

             
                await client.From<SupabaseTransaction>().Upsert(supabaseTransactions);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Upload transactions failed: {ex.Message}");
            }
        }
        public async Task<(bool Success, string Error)> UploadRecurringTransactionsAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

               
                var localRecurring = await _recurringService.GetAllRecurringTransactionsAsync(userId);

                if (!localRecurring.Any())
                    return (true, string.Empty);
               
                await client
                   .From<SupabaseRecurringTransaction>()
                   .Where(x => x.UserId == userId)
                   .Delete();

                var supabaseRecurring = localRecurring.Select(lr => new SupabaseRecurringTransaction
                {
                    Id = lr.Id,
                    UserId = lr.UserId,
                    CategoryId = lr.CategoryId,
                    Name = lr.Name,
                    Amount = lr.Amount,
                    Type = lr.Type,
                    Frequency = lr.Frequency,
                    DayOfMonth = lr.DayOfMonth,
                    DayOfWeek = lr.DayOfWeek,
                    IsActive = lr.IsActive,
                    LastProcessedDate = lr.LastProcessedDate,
                    NextDueDate = lr.NextDueDate,
                    CreatedAt = lr.CreatedAt
                }).ToList();

                await client.From<SupabaseRecurringTransaction>().Upsert(supabaseRecurring);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Upload recurring transactions failed: {ex.Message}");
            }
        }


        public async Task<(bool Success, string Error)> UploadUserAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();
                var localUser = await _userService.GetUserByIdAsync(userId);
                if (localUser == null)
                {
                    return (false, "Local user not found");
                }

               
                var supabaseUser = new SupabaseUser
                {
                    Id = localUser.Id,
                    Email = localUser.Email,
                    DisplayName = localUser.DisplayName,
                    CreatedAt = localUser.CreatedAt,                 
                };


                await client.From<SupabaseUser>().Upsert(supabaseUser);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        public async Task<(bool Success, string Error)> UploadAllDataAsync(string userId)
        {
            try
            {
           
                var (catSuccess, catError) = await UploadCategoriesAsync(userId);
                if (!catSuccess) return (false, catError);

                var (transSuccess, transError) = await UploadTransactionsAsync(userId);
                if (!transSuccess) return (false, transError);

                var (recSuccess, recError) = await UploadRecurringTransactionsAsync(userId);
                if (!recSuccess) return (false, recError);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Upload all data failed: {ex.Message}");
            }
        }




        public async Task<(bool Success, string Error)> DownloadCategoriesAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

               
                var response = await client.From<SupaBaseCategory>()
                    .Where(x => x.UserId == userId)
                    .Get();

                if (response.Models == null || !response.Models.Any())
                    return (true, string.Empty);

                var localCategories = response.Models.Select(sc => new Category
                {
                    Id = sc.Id,
                    UserId = sc.UserId,
                    Name = sc.Name,
                    Icon = sc.Icon,
                    Color = sc.Color,
                    MonthlyBudget = sc.MonthlyBudget,
                    Type = sc.Type,
                    CreatedAt = sc.CreatedAt
                }).ToList();

                foreach (var category in localCategories)
                {
                    var existing = await _categoryService.GetCategoryByIdAsync(category.Id);
                    if (existing != null)
                    {
                        await _categoryService.UpdateCategoryAsync(category);
                    }
                    else
                    {
                        await _categoryService.CreateCategoryAsync(category);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Download categories failed: {ex.Message}");
            }
        }


        public async Task<(bool Success, string Error)> DownloadTransactionsAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

                
                var response = await client.From<SupabaseTransaction>()
                    .Where(x => x.UserId == userId)
                    .Get();

                if (response.Models == null || !response.Models.Any())
                    return (true, string.Empty);

            

                var localTransactions = response.Models.Select(st => new TransactionRecord
                {
                    Id = st.Id,
                    UserId = st.UserId,
                    CategoryId = st.CategoryId,
                    Name = st.Name,
                    Amount = st.Amount,
                    Type = st.Type,
                    Description = st.Description,
                    Date = st.Date,
                    PhotoPath = st.PhotoPath,
                    CreatedAt = st.CreatedAt,
                    UpdatedAt = st.UpdatedAt
                }).ToList();

              
                foreach (var transaction in localTransactions)
                {
                    var existing = await _transactionService.GetTransactionByIdAsync(transaction.Id);
                    if (existing != null)
                    {
                        await _transactionService.UpdateTransactionAsync(transaction);
                    }
                    else
                    {
                        await _transactionService.CreateTransactionRecordAsync(transaction);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Download transactions failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> DownloadRecurringTransactionsAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

                var response = await client.From<SupabaseRecurringTransaction>()
                    .Where(x => x.UserId == userId)
                    .Get();

                if (response.Models == null || !response.Models.Any())
                    return (true, string.Empty);

              

                var localRecurring = response.Models.Select(sr => new RecurringTransaction
                {
                    Id = sr.Id,
                    UserId = sr.UserId,
                    CategoryId = sr.CategoryId,
                    Name = sr.Name,
                    Amount = sr.Amount,
                    Type = sr.Type,
                    Frequency = sr.Frequency,
                    DayOfMonth = sr.DayOfMonth,
                    DayOfWeek = sr.DayOfWeek,
                    IsActive = sr.IsActive,
                    LastProcessedDate = sr.LastProcessedDate,
                    NextDueDate = sr.NextDueDate,
                    CreatedAt = sr.CreatedAt
                }).ToList();

                foreach (var recurring in localRecurring)
                {
                    var existing = await _recurringService.GetRecurringTransactionByIdAsync(recurring.Id);
                    if (existing != null)
                    {
                        await _recurringService.UpdateRecurringTransactionAsync(recurring);
                    }
                    else
                    {
                        await _recurringService.CreateRecurringTransactionAsync(recurring);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Download recurring transactions failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> DownloadUserAsync(string userId)
        {
            try
            {
                var client = await _authService.GetClientAsync();

                System.Diagnostics.Debug.WriteLine("📥 Downloading profile...");

                var response = await client
                    .From<SupabaseUser>()
                    .Where(x => x.Id == userId)
                    .Single();
                   

                if (response == null)
                {
                    return (false, "Profile not found");
                }

                var localUser = await _userService.GetUserByIdAsync(userId);

                if (localUser == null)
                {
                    
                    localUser = new User
                    {
                        Id = response.Id,
                        Email = response.Email,
                        DisplayName = response.DisplayName,
                        CreatedAt = response.CreatedAt,
                        LastLoginAt = DateTime.Now
                    };
                    await _userService.CreateUserAsync(localUser);
                }
                else
                {
                    await _userService.UpdateUserAsync(localUser);
                }
                CurrentUser.SetDisplayName(localUser.DisplayName);
                CurrentUser.SetEmail(localUser.Email);
                CurrentUser.SetUserId(localUser.Id);      
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Profile download failed: {ex.Message}");
                return (false, ex.Message);
            }
        }


        public async Task<(bool Success, string Error)> DownloadAllDataAsync(string userId)
        {
            try
            {
                
                var (catSuccess, catError) = await DownloadCategoriesAsync(userId);
                if (!catSuccess) return (false, catError);

                var (transSuccess, transError) = await DownloadTransactionsAsync(userId);
                if (!transSuccess) return (false, transError);

                var (recSuccess, recError) = await DownloadRecurringTransactionsAsync(userId);
                if (!recSuccess) return (false, recError);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Download all data failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Error)> SyncAllDataAsync(string userId)
        {
            if (_isSyncing)
            {
                return (false, "Sync already in progress");
            }

          
            if (string.IsNullOrEmpty(userId))
                return (false, "Not authenticated");

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return (false, "No internet connection");
            }
            try
            {                    
                _isSyncing = true;
                await TryRefreshTokenAsync();
                var (downloadSuccess, downloadError) = await DownloadAllDataAsync(userId);
                if (!downloadSuccess) return (false, $"Download failed: {downloadError}");
                var (uploadSuccess, uploadError) = await UploadAllDataAsync(userId);
                if (!uploadSuccess) return (false, $"Upload failed: {uploadError}");

           

                var (downloadUserSuccess, downloadUserError) = await DownloadUserAsync(userId);

            
                 if (downloadUserError.Contains("not found") || downloadUserError.Contains("Profile not found"))
                {
                  
                    var (uploadUserSuccess, uploadUserError) = await UploadUserAsync(userId);
                }

                Preferences.Set(LastSyncKey, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Sync failed: {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
            }
        }

    

        public DateTime? GetLastSyncTime()
        {
            var lastSyncString = Preferences.Get(LastSyncKey, null);
            if (string.IsNullOrEmpty(lastSyncString))
                return null;

            if (DateTime.TryParse(lastSyncString, out var lastSync))
                return lastSync;

            return null;
        }


        public bool IsAutoSyncEnabled()
        {
            return Preferences.Get(AutoSyncEnabledKey, true);
        }
        public void SetAutoSyncEnabled(bool enabled)
        {
            Preferences.Set(AutoSyncEnabledKey, enabled);
        }
        public bool ShouldAutoSync()
        {
            var lastSync = GetLastSyncTime();
            var autoSyncEnabled = IsAutoSyncEnabled();

            return SyncHelper.ShouldAutoSync(lastSync, autoSyncEnabled);
        }

        public string GetLastSyncDisplayText()
        {
            var lastSync = GetLastSyncTime();

            
            return SyncHelper.GetLastSyncDisplayText(lastSync);
        }

        private async Task TryRefreshTokenAsync()
        {
            try
            {
                var client = await _authService.GetClientAsync();
                var session = client.Auth.CurrentSession;

                if (session.ExpiresAt != null)
                {
                    var expiresIn = session.ExpiresAt() - DateTime.UtcNow;

                    if (expiresIn.TotalMinutes < 5)
                    {
                        await client.Auth.RefreshSession();
                     
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

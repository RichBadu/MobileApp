using Mde.Project.Mobile.Domain.Helpers;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Views.Auth;

namespace Mde.Project.Mobile
{
    public partial class App : Application
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly ISyncService _syncService;
        public App(ISupabaseAuthService supabaseAuthService, ISyncService syncService)
        {
            InitializeComponent();
            _supabaseAuthService = supabaseAuthService;
            _syncService = syncService;
            MainPage = new AppShell();
        
        }

        protected override async void OnStart()
        {
            base.OnStart();
        
            try
            {
               
                await _supabaseAuthService.InitializeAsync();
             
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Supabase init failed (app will work offline): {ex.Message}");   
            }

           
            if (CurrentUser.IsLoggedIn())
            {
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    await CheckAndAutoSyncAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("📵 Offline - skipping auto-sync");
                }

                await Shell.Current.GoToAsync("//home");
            }
            else
            {
               
                await Shell.Current.GoToAsync("//login");
            }
        }

        private async Task CheckAndAutoSyncAsync()
        {
            try
            {

                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("Offline - skipping auto-sync");
                    return;
                }

                if (_syncService.ShouldAutoSync())
                {
                    var userId = CurrentUser.GetUserId();
                    if (string.IsNullOrEmpty(userId))
                        return;

                    System.Diagnostics.Debug.WriteLine("Auto-sync on app startup...");
                    var (success, error) = await _syncService.SyncAllDataAsync(userId);

                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine($"App startup auto-sync failed: {error}");

                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Auto-sync not needed (recently synced)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App startup auto-sync error: {ex.Message}");
            }
        }
    }
}

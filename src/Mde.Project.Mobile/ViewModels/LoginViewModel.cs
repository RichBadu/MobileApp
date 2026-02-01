using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = Mde.Project.Mobile.Core.Models.User;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly ISyncService _syncService;


        public LoginViewModel(IUserService userService, ISupabaseAuthService supabaseAuthService, ISyncService syncService)
        {
            _userService = userService;
            _supabaseAuthService = supabaseAuthService;
            _syncService = syncService;
        }

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string error = string.Empty;

        [RelayCommand]
        async Task Login()
        {
            if (string.IsNullOrEmpty(Email))
            {
                Error = "Email is required.";
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                Error = "Password is required.";
                return;
            }

            try
            {
                IsBusy = true;
                Error = string.Empty;

                var (success, userId, error) = await _supabaseAuthService.LoginAsync(Email, Password);

                if (success)
                {
                    var user = await _userService.GetUserByIdAsync(userId);
                  

                    if (user != null)
                    {
                        await _userService.UpdateLastLoginAsync(user.Id);
                    }
                      
                    var (syncSuccess, syncError) = await _syncService.SyncAllDataAsync(userId);

                    if (!syncSuccess)
                    {
                        await Shell.Current.DisplayAlert("Sync",
                        "Login successful, but sync failed. You can manually sync later.",
                        "OK");
                    }
                
                    await Shell.Current.GoToAsync("///home");
                }
                else
                {
                    Error = error;
                }
            }
            catch (Exception ex)
            {
                Error = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }




        }

        [RelayCommand]
        async Task NavigateToRegister()
        {
            await Shell.Current.GoToAsync("//register");
        }
    }
}

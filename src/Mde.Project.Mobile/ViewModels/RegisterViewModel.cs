using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ISupabaseAuthService _supabaseAuthService;
       
        public RegisterViewModel(IUserService userService, ICategoryService categoryService, ISupabaseAuthService supabaseAuthService)
        {
            _userService = userService;
            _categoryService = categoryService;
            _supabaseAuthService = supabaseAuthService;
        }
        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string displayName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string error = string.Empty;

        [RelayCommand]
        async Task Register()
        {
            if (!await Validator())
                return;
            try
            {
                IsBusy = true;
                Error = string.Empty;

                var (success, userId, error) = await _supabaseAuthService.RegisterAsync(Email, Password, DisplayName);

                if (success)
                {
                    var user = new User
                    {
                        Id = userId,
                        Email = Email,
                        DisplayName = DisplayName,
                        CreatedAt = DateTime.Now
                    };
                    var createdUser = await _userService.CreateUserAsync(user);
                    await _categoryService.CreateDefaultCategoriesAsync(userId);
                    await Shell.Current.DisplayAlertAsync("Success", "Account created successfully!", "OK");
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    Error = error;
                }
            }catch(Exception ex)
            {
                Error = $"Registration failed: {ex.Message}";
            }
        }
        
        [RelayCommand]
        async Task NavigateToLogin()
        {
            await Shell.Current.GoToAsync("//login");
        }

        private async Task<bool>  Validator()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                Error = "Display name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                Error = "Email is required.";
                return false;
            }
            if (!IsValidEmail(Email))
            {
                Error = "Please enter a valid email address.";
                return false;
            }

            var exisitingUser = await _userService.GetUserByEmailAsync(Email);
            if (exisitingUser != null)
            {
                Error = "This email is already registered";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Error = "Password is required.";
                return false;
            }
            if (Password.Length < 6)
            {
                Error = "Password must be at least 6 characters long.";
                return false;
            }
            if (Password != ConfirmPassword)
            {
                Error = "Passwords do not match.";
                return false;
            }

            return true;
        }





        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}

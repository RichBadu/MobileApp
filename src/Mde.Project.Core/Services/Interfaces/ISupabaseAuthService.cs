using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface ISupabaseAuthService
    {
        Task<(bool Success, string UserId, string Error)> LoginAsync(string email, string password);
        Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string password, string displayName);
        Task LogoutAsync();
        string? GetCurrentUserId();
        bool IsLoggedIn();
        Task InitializeAsync();
        Task<Supabase.Client> GetClientAsync();
    }
}

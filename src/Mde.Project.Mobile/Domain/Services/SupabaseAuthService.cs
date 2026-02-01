using Mde.Project.Mobile.Core.Helpers;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Mde.Project.Mobile.Domain.Helpers;
using Supabase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Domain.Services
{
    public class SupabaseAuthService : ISupabaseAuthService
    {
        private Client? _supabase;
        private bool _initialized = false;

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;
          
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            _supabase = new Client(SupabaseSettings.Url, SupabaseSettings.AnonKey, options);
          
            try
            {
                await _supabase.InitializeAsync();
                _initialized = true;
                System.Diagnostics.Debug.WriteLine("Supabase initialized successfully");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Supabase initialization failed: {ex.Message}");
            }

        }

        public async Task<(bool Success, string UserId, string Error)> LoginAsync(string email, string password)
        {
            try
            {
                await InitializeAsync();
                var session = await _supabase.Auth.SignIn(email, password);
                if (session?.User != null)
                {
                    CurrentUser.SetUserId(session.User.Id);
                    CurrentUser.SetEmail(email);
                    Preferences.Set("supabase_token", session.AccessToken);
                    Preferences.Set("supabase_refresh_token", session.RefreshToken);

            
                    var savedAccess = Preferences.Get("supabase_token", null);
                    var savedRefresh = Preferences.Get("supabase_refresh_token", null);

                    return (true, session.User.Id, string.Empty);
                }
                return (false, string.Empty, "Login failed");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("network") || ex.Message.Contains("connection"))
                {
                    return (false, string.Empty, "No internet connection. Please check your network.");
                }
                return (false, string.Empty, GetErrorMessage(ex));
            }
        }
        public async Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string password, string displayName)
        {
            try
            {
                await InitializeAsync();
                var session = await _supabase.Auth.SignUp(email, password);

                if (session?.User != null)
                {
                    CurrentUser.SetUserId(session.User.Id);
                    CurrentUser.SetEmail(email);
                    Preferences.Set("supabase_token", session.AccessToken);
                    Preferences.Set("supabase_refresh_token", session.RefreshToken);
                    return (true, session.User.Id, string.Empty);
                }
                return (false, string.Empty, "Registration failed");
            }catch(Exception ex)
            {
                return (false, string.Empty, GetErrorMessage(ex));
            }
          
        }
        public async Task LogoutAsync()
        {

            if (_supabase != null)
            {
                await _supabase.Auth.SignOut();
            }
            Preferences.Remove("supabase_token");
            Preferences.Remove("supabase_refresh_token");
            CurrentUser.Clear();
        }
        public string? GetCurrentUserId()
        {
            return CurrentUser.GetUserId();
        }
        public bool IsLoggedIn()
        {
            return CurrentUser.IsLoggedIn();
        }

        public async Task<Supabase.Client> GetClientAsync()
        {
        

            await InitializeAsync();

            if (_supabase == null)
            {
             
                throw new InvalidOperationException("Supabase client not initialized");
            }

       

            var currentSession = _supabase.Auth.CurrentSession;

      


            if (currentSession == null || IsSessionExpired(currentSession))
            {

                var accessToken = Preferences.Get("supabase_token", null);
                var refreshToken = Preferences.Get("supabase_refresh_token", null);



                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    

                    try
                    {
                        await _supabase.Auth.SetSession(accessToken, refreshToken);

                        currentSession = _supabase.Auth.CurrentSession;

                        if (currentSession != null)
                        {


                            if (IsSessionExpired(currentSession))
                            {
                               
                                await _supabase.Auth.RefreshSession();
                              

                                currentSession = _supabase.Auth.CurrentSession;
                               
                            }
                            else
                            {
                                var expiresIn = currentSession.ExpiresAt() - DateTime.UtcNow;
                        
                            }
                        }
                        else
                        {
                         
                        }
                    }
                    catch (Exception ex)
                    {
             

                        Preferences.Remove("supabase_access_token");
                        Preferences.Remove("supabase_refresh_token");

                        throw new InvalidOperationException("Session expired. Please login again.", ex);
                    }
                }
                else
                {
    
                    throw new InvalidOperationException("No stored session. Please login.");
                }
            }
            else
            {
                var expiresIn = currentSession.ExpiresAt() - DateTime.UtcNow;
            }



            return _supabase;
        }
        private string GetErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("invalid login credentials"))
                return "Invalid email or password";
     
            if (message.Contains("user already registered"))
                return "Email already registered";
            if (message.Contains("password") && message.Contains("short"))
                return "Password should be at least 6 characters";
            if (message.Contains("invalid email"))
                return "Invalid email address";

            return $"Authentication error: {ex.Message}";
        }

        private bool IsSessionExpired(Supabase.Gotrue.Session session)
        {
            if (session.ExpiresAt == null)
                return true;

            return false;
        }



    }
}

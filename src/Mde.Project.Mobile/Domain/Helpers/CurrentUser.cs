using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Domain.Helpers
{
    public static class CurrentUser
    {
        private const string UserIdKey = "user_id";
        private const string EmailKey = "user_email";
        private const string DisplayNameKey = "user_displayname";

        public static void SetUserId(string userId)
        {
            Preferences.Set(UserIdKey, userId);
        }
        public static void SetEmail(string email)
        {
            Preferences.Set(EmailKey, email);
        }
        public static void SetDisplayName(string displayName)
        {
            Preferences.Set(DisplayNameKey, displayName);
        }

        public static string? GetUserId()
        {
            return Preferences.Get(UserIdKey, null);
        }
        public static string? GetEmail()
        {
            return Preferences.Get(EmailKey, null);
        }
        public static string? GetDisplayName()
        {
            return Preferences.Get(DisplayNameKey, null);
        }
        public static void Clear()
        {
            Preferences.Remove(UserIdKey);
            Preferences.Remove(EmailKey);
            Preferences.Remove(DisplayNameKey);
            Preferences.Remove("supabase_token");
            Preferences.Remove("supabase_refresh_token");
        }

        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetUserId());
        }
    }
}

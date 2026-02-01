using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Core.Helpers
{
    public static class SyncHelper
    {
        public static bool ShouldAutoSync(DateTime? lastSyncTime, bool autoSyncEnabled)
        {
            if (!autoSyncEnabled)
                return false;

            return true;
        }

        public static string GetLastSyncDisplayText(DateTime? lastSyncTime)
        {
            if (lastSyncTime == null)
                return "Never synced";

            var timeSpan = DateTime.Now - lastSyncTime.Value;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";

            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";

            return lastSyncTime.Value.ToString("dd MMM yyyy");
        }
    }
}


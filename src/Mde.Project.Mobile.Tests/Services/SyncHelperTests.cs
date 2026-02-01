using Mde.Project.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Tests.Services
{
    public class SyncHelperTests
    {
        [Fact]
        public void ShouldAutoSync_NeverSynced_ReturnsTrue()
        {
            // Arrange
            DateTime? lastSyncTime = null;
            bool autoSyncEnabled = true;

            // Act
            bool result = SyncHelper.ShouldAutoSync(lastSyncTime, autoSyncEnabled);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(-12, true, false)]   // 12 hours ago, enabled → false (too soon)
        [InlineData(-25, true, true)]    // 25 hours ago, enabled → true (time to sync!)
        [InlineData(-48, true, true)]    // 48 hours ago, enabled → true
        [InlineData(-25, false, false)]  // 25 hours ago, disabled → false (auto-sync off)
        [InlineData(-12, false, false)]  // 12 hours ago, disabled → false
        public void ShouldAutoSync_VariousScenarios_ReturnsExpectedResult(double hoursAgo, bool autoSyncEnabled,bool expectedResult)
        {
            // Arrange
            DateTime? lastSyncTime = DateTime.Now.AddHours(hoursAgo);

            // Act
            bool result = SyncHelper.ShouldAutoSync(lastSyncTime, autoSyncEnabled);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ShouldAutoSync_Exactly24HoursAgo_ReturnsTrue()
        {
            // Arrange
            DateTime? lastSyncTime = DateTime.Now.AddHours(-24).AddSeconds(-1);  
            bool autoSyncEnabled = true;

            // Act
            bool result = SyncHelper.ShouldAutoSync(lastSyncTime, autoSyncEnabled);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetLastSyncDisplayText_NeverSynced_ReturnsNeverSynced()
        {
            // Arrange
            DateTime? lastSyncTime = null;

            // Act
            string result = SyncHelper.GetLastSyncDisplayText(lastSyncTime);

            // Assert
            Assert.Equal("Never synced", result);
        }

        [Theory]
        [InlineData(-0.5, "Just now")]         
        [InlineData(-5, "5 min ago")]            
        [InlineData(-30, "30 min ago")]       
        [InlineData(-90, "1 hours ago")]         
        [InlineData(-300, "5 hours ago")]        
        [InlineData(-1440, "1 days ago")]       
        [InlineData(-4320, "3 days ago")]       
        public void GetLastSyncDisplayText_VariousTimes_ReturnsCorrectFormat(double minutesAgo,string expectedText)
        {
            // Arrange
            DateTime? lastSyncTime = DateTime.Now.AddMinutes(minutesAgo);

            // Act
            string result = SyncHelper.GetLastSyncDisplayText(lastSyncTime);

            // Assert
            Assert.Equal(expectedText, result);
        }

        [Fact]
        public void GetLastSyncDisplayText_MoreThan7DaysAgo_ReturnsDateFormat()
        {
            // Arrange
            DateTime? lastSyncTime = new DateTime(2024, 12, 1, 10, 30, 0);

            // Act
            string result = SyncHelper.GetLastSyncDisplayText(lastSyncTime);

            // Assert
            Assert.Equal("01 Dec 2024", result);
        }

    }
}

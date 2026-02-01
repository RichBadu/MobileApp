using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services;
using Mde.Project.Mobile.Core.Services.Interfaces;
using Moq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Tests.Services
{
    public class UserServiceTests
    {
        private SQLiteAsyncConnection _testDb;
        private Mock<IDatabaseService> _mockDatabaseService;
        private UserService _userService;

        private async Task SetupAsync()
        {
            _testDb = new SQLiteAsyncConnection(":memory:");
            await _testDb.CreateTableAsync<User>();

            _mockDatabaseService = new Mock<IDatabaseService>();
            _mockDatabaseService
                .Setup(m => m.GetDatabaseAsync())
                .ReturnsAsync(_testDb);

            _userService = new UserService(_mockDatabaseService.Object);
        }

        [Fact]
        public async Task CreateUser_NewUser_SavesCorrectly()
        {
            await SetupAsync();
            //arrange
            var user = new User { DisplayName = "test", Email = "test@test.be" };
  
            //act
            var result = await _userService.CreateUserAsync(user);
            //assert
            Assert.NotEqual(result.Id, Guid.Empty.ToString());

            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ExistingUser_ReturnsUser()
        {
            await SetupAsync();

            var userId = "test";
            // Arrange
            var user = new User
            {
                Id = userId,
                Email = "test@test.be"
            };

            await _testDb.InsertAsync(user);

            // Act
            var result = await _userService.GetUserByEmailAsync("test@test.be");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
     

            await _testDb.CloseAsync();
        }

        [Fact]
        public async Task GetUserByEmailAsync_NonExistingUser_ReturnsNull()
        {
            await SetupAsync();

            var userId = "test";
            // Arrange
            var user = new User
            {
                Id = userId,
                Email = "test@test.be"
            };

            await _testDb.InsertAsync(user);

            // Act
            var result = await _userService.GetUserByEmailAsync("wrong");

            // Assert
            Assert.Null(result);
       
            await _testDb.CloseAsync();
        }



        [Fact]
        public async Task GetUserByEmailAsync_ExistingUser_ReturnsDisplayName()
        {
            await SetupAsync();

            var userId = "test";
            var displayname = "tester";
            // Arrange
            var user = new User
            {
                Id = userId,
                Email = "test@test.be",
                DisplayName = displayname
            };

            await _testDb.InsertAsync(user);

            // Act
            var result = await _userService.GetUserByEmailAsync("test@test.be");

            // Assert
            Assert.Equal(displayname, result.DisplayName);


            await _testDb.CloseAsync();
        }


    }
}

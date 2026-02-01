using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services
{
    public class UserService : IUserService
    {

        private readonly IDatabaseService _databaseService;


        public UserService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var db = await _databaseService.GetDatabaseAsync();
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }
            await db.InsertAsync(user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var user = await GetUserByIdAsync(userId);

            if (user == null)
                return false;

            await db.DeleteAsync(user);
            return true;

        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var db = await _databaseService.GetDatabaseAsync();
            return await db.Table<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();
            return await db.Table<User>()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var user = await GetUserByIdAsync(userId);

            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
                await db.UpdateAsync(user);
                return true;
            }
            throw new ArgumentException($"Failed to update login user");
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var db = await _databaseService.GetDatabaseAsync();
            var userToUpdate = await GetUserByIdAsync(user.Id);
            if (userToUpdate == null) throw new ArgumentException("this user could not be found");

            userToUpdate.Email = user.Email;
            userToUpdate.DisplayName = user.DisplayName;
            userToUpdate.LastLoginAt = DateTime.Now;

            await db.UpdateAsync(userToUpdate);
            return true;
        }

     
    }
}

using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface IUserService
    {
        //create
        Task<User> CreateUserAsync(User user);

        //read
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);

        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateLastLoginAsync(string userId);

        Task<bool> DeleteUserAsync(string userId);
    }
}

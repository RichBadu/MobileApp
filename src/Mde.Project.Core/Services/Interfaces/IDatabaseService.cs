using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface IDatabaseService
    {
        Task<SQLiteAsyncConnection> GetDatabaseAsync();
        Task InitializeDatabaseAsync();
    }
}

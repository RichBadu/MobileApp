using Mde.Project.Mobile.Core.Models;
using Mde.Project.Mobile.Core.Services.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Domain.Services
{
    public class DatabaseService : IDatabaseService
    {
        private const string _DbName = "trackeroo.db3";
        private readonly string _connectionPath;
        private SQLiteAsyncConnection? _database;

        public DatabaseService()
        {
            _connectionPath = Path.Combine(FileSystem.AppDataDirectory, _DbName);
        }
        public async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (_database != null)
                return _database;

            _database = new SQLiteAsyncConnection(_connectionPath);
            await InitializeDatabaseAsync();
            return _database;

        }

        public async Task InitializeDatabaseAsync()
        {
            if (_database == null)
                return;
            try
            {
                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Category>();
                await _database.CreateTableAsync<TransactionRecord>();
                await _database.CreateTableAsync<RecurringTransaction>();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"something went wrong while creating the database {ex.Message}");
            }

        }  
        
    }
}

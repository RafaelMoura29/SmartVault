using SmartVault.Shared.Configuration;
using System;
using System.Data.SQLite;

namespace SmartVault.Shared.Data
{
    public class DbContext : IDbContext
    {
        public readonly IAppSettings _settings;
        private SQLiteConnection _connection;

        public DbContext()
        {
            _settings = new AppSettings();
        }

        public void InitDatabase()
        {
            SQLiteConnection.CreateFile(_settings.DatabaseFileName);
        }

        public SQLiteConnection GetConnection()
        {
            if (_connection is null)
            {
                _connection = new SQLiteConnection(_settings.DefaultConnection);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        public void Dispose()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }

            _connection.Dispose();
        }
    }
}

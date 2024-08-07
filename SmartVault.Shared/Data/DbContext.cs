using SmartVault.Shared.Configuration;
using System.Data.SQLite;
using System.IO;

namespace SmartVault.Shared.Data
{
    public class DbContext : IDbContext
    {
        public readonly IAppSettings Settings;
        private readonly SQLiteConnection _connection;

        public DbContext()
        {
            Settings = new AppSettings();
            var a = Path.GetFullPath(Settings.DatabaseFileName);
            _connection = new SQLiteConnection(Settings.DefaultConnection);
        }

        public SQLiteConnection GetConnection()
        {
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

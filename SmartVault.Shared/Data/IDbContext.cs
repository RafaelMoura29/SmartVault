using System;
using System.Data.SQLite;

namespace SmartVault.Shared.Data
{
    public interface IDbContext : IDisposable
    {
        void InitDatabase();
        SQLiteConnection GetConnection();
    }
}

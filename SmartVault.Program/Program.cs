using Dapper;
using SmartVault.Program.BusinessObjects;
using SmartVault.Shared.Configuration;
using SmartVault.Shared.Data;
using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.IO;
using System.Text;
using DbContext = SmartVault.Shared.Data.DbContext;

namespace SmartVault.Program
{
    partial class Program
    {
        private static IDbContext _context;
        private static IAppSettings _settings;
        private static SQLiteConnection _connection;

        static Program()
        {
            _context = new DbContext();
            _settings = new AppSettings();
            _connection = _context.GetConnection();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            //WriteEveryThirdFileToFile("2");
            WriteEveryThirdFileToFile(args[0]);
            GetAllFileSizes();
        }

        private static void GetAllFileSizes()
        {
            long totalFileSize = 0;
            var filesSize = new ConcurrentDictionary<string, long>(StringComparer.Ordinal);

            var documents = _connection.Query<Document>("select FilePath from Document;");
            foreach (var document in documents)
            {
                if (filesSize.TryGetValue(document.FilePath, out long fileSize))
                {
                    totalFileSize += fileSize;
                }
                else
                {
                    var file = new FileInfo(document.FilePath);
                    filesSize.AddOrUpdate(document.FilePath, file.Length, (x, s) => s = file.Length);
                    totalFileSize += file.Length;
                }
             }

            Console.WriteLine($"Total files size: {totalFileSize} bytes");
        }

        private static void WriteEveryThirdFileToFile(string accountId)
        {
            var documents = _connection.Query<Document>("select * from Document where AccountId = @accountId and Id % 3 = 0", new { accountId });
            var content = new StringBuilder();
                
            foreach (var document in documents)
            {
                var file = new FileInfo(document.FilePath);
                string fileContent = File.ReadAllText(file.FullName);

                content.Append(fileContent);
            }

            File.WriteAllText(string.Format(_settings.OutputFilePath, accountId), content.ToString());
        }
    }
}
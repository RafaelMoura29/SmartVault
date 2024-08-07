using Dapper;
using SmartVault.Program.BusinessObjects;
using SmartVault.Shared.Configuration;
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
        private readonly static SQLiteConnection _connection;
        private readonly static IAppSettings _settings;

        static Program()
        {
            _connection = new DbContext().GetConnection();
            _settings = new AppSettings();
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
            long totalFilesSize = 0;
            var filesSize = new ConcurrentDictionary<string, long>(StringComparer.Ordinal);

            var documents = _connection.Query<Document>("select FilePath from Document;");
            foreach (var document in documents)
            {
                if (filesSize.TryGetValue(document.FilePath, out long fileSize))
                {
                    totalFilesSize += fileSize;
                }
                else
                {
                    var file = new FileInfo(document.FilePath);
                    filesSize.AddOrUpdate(document.FilePath, file.Length, (x, s) => s = file.Length);
                    totalFilesSize += file.Length;
                }
             }

            Console.WriteLine($"GetAllFileSizes: Total files size - {totalFilesSize} Bytes");
        }

        private static void WriteEveryThirdFileToFile(string accountId)
        {
            var documents = _connection.Query<Document>("select * from Document where AccountId = @accountId and Id % 3 = 0", new { accountId });
            var content = new StringBuilder();
                
            foreach (var document in documents)
            {
                string fileContent = File.ReadAllText(document.FilePath);

                content.Append(fileContent);
            }

            File.WriteAllText(string.Format(_settings.OutputFilePath, accountId), content.ToString());
            Console.WriteLine($"WriteEveryThirdFileToFile: Executed successfully");
        }
    }
}
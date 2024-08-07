using Dapper;
using Newtonsoft.Json;
using SmartVault.Library;
using SmartVault.Shared.Configuration;
using SmartVault.Shared.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartVault.DataGeneration
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
            File.WriteAllText("TestDoc.txt", $"This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}");
            
            CreateTables();
                
            GenereteFakeDataAsync();

            var accountData = _connection.Query("SELECT COUNT(*) FROM Account;");
            Console.WriteLine($"AccountCount: {JsonConvert.SerializeObject(accountData)}");
            var documentData = _connection.Query("SELECT COUNT(*) FROM Document;");
            Console.WriteLine($"DocumentCount: {JsonConvert.SerializeObject(documentData)}");
            var userData = _connection.Query("SELECT COUNT(*) FROM User;");
            Console.WriteLine($"UserCount: {JsonConvert.SerializeObject(userData)}");
        }

        private static void GenereteFakeDataAsync()
        {
            var documentPath = new FileInfo("TestDoc.txt").FullName;
            var fileLength = new FileInfo(documentPath).Length;
            var randomDayIterator = RandomDay().GetEnumerator();

            using (var transaction = _connection.BeginTransaction())
            {
                Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, userId =>
                {
                    randomDayIterator.MoveNext();
                    GenerateUserData(userId, documentPath, fileLength, randomDayIterator.Current);
                });

                transaction.Commit();
            }
        }

        private static void GenerateUserData(int userId, string documentPath, long fileLength, DateTime birthDate)
        {
            var query = new StringBuilder();
            query.Append($"INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password) VALUES('{userId}','FName{userId}','LName{userId}','{birthDate:yyyy-MM-dd}','{userId}','UserName-{userId}','e10adc3949ba59abbe56e057f20f883e');");
            query.Append($"INSERT INTO Account (Id, Name) VALUES('{userId}','Account{userId}');");

            query.Append(@$"WITH RECURSIVE
                            sequence(x) AS (
                                SELECT 0
                                UNION ALL
                                SELECT x + 1 FROM sequence WHERE x < 9999
                            )
                            INSERT INTO Document (Name, FilePath, Length, AccountId)
                            SELECT 
                                'Document' || {userId} || '-' || x || '.txt' AS Name,
                                '{documentPath}' AS FilePath,
                                '{fileLength}' AS Length,
                                '{userId}' AS AccountId
                            FROM sequence;
            ");

            _connection.Execute(query.ToString());

            query.Clear();
        }

        private static void CreateTables()
        {
            var query = new StringBuilder();
            var files = Directory.GetFiles(@"..\..\..\..\BusinessObjectSchema");
            foreach (var file in files)
            {
                var serializer = new XmlSerializer(typeof(BusinessObject));
                var businessObject = serializer.Deserialize(new StreamReader(file)) as BusinessObject;
                query.AppendLine(businessObject?.Script);
            }

            _connection.Execute(query.ToString());
        }

        static IEnumerable<DateTime> RandomDay()
        {
            DateTime start = new DateTime(1985, 1, 1);
            Random gen = new Random();
            int range = (DateTime.Today - start).Days;
            while (true)
                yield return start.AddDays(gen.Next(range));
        }
    }
}
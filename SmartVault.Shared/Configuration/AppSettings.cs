using Microsoft.Extensions.Configuration;
using System.IO;

namespace SmartVault.Shared.Configuration
{
    public class AppSettings : IAppSettings
    {
        private readonly IConfigurationRoot _configuration;

        public AppSettings()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
        }

        public string DefaultConnection =>
           string.Format(_configuration.GetSection("ConnectionStrings:DefaultConnection").Value, DatabaseFileName);

        public string DatabaseFileName => _configuration["DatabaseFileName"];
        public string OutputFilePath => _configuration["OutputFilePath"];
    }
}

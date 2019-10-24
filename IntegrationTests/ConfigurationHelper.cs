using Microsoft.Extensions.Configuration;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationTests
{
    public class ConfigurationHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.test.json")
                .Build();
        }

        public static ConfigurationModel GetApplicationConfiguration(string outputPath)
        {
            var configuration = new ConfigurationModel();

            var iConfig = GetIConfigurationRoot(outputPath);

            iConfig
                .GetSection("config")
                .Bind(configuration);

            return configuration;
        }
    }
}

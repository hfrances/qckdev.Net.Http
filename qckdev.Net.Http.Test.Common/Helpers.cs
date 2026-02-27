using System;
using System.IO;
using System.Net;

#if NETCOREAPP
using Microsoft.Extensions.Configuration;
#endif

namespace qckdev.Net.Http.Test.Common
{

#if NET40_OR_GREATER || NETCOREAPP
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#else
#endif
    public static class Helpers
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty")]
        public static void SetDefaultSecurityProtocol()
        {
#if SET_SECURITY_PROTOCOL35
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                (SecurityProtocolType)3072;
#elif SET_SECURITY_PROTOCOL
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;
#else
#endif
        }

        public static Configuration.Settings GetSettings(string environment = "Development")
        {
            Configuration.Settings settings;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

#if NETCOREAPP
            var builder = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{environment}.json", true, true)
                    .AddEnvironmentVariables();

            var config = builder.Build();
            settings = config.Get<Configuration.Settings>();
#else
            var fileName = Path.Combine(baseDirectory, "appsettings.json");
            var fileNameByEnv = Path.Combine(baseDirectory, $"appsettings.{environment}.json");
            settings = new Configuration.Settings();

            if (File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    Newtonsoft.Json.JsonConvert.PopulateObject(reader.ReadToEnd(), settings);
                }
            }
            if (!string.IsNullOrEmpty(environment?.Trim()) && File.Exists(fileNameByEnv))
            {
                using (var reader = new StreamReader(fileNameByEnv))
                {
                    Newtonsoft.Json.JsonConvert.PopulateObject(reader.ReadToEnd(), settings);
                }
            }
#endif
            settings = LocalTestServiceManager.NormalizeSettingsForCurrentFramework(settings);
            return settings;
        }
    }
}

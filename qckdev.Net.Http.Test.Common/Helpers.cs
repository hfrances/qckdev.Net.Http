using System.Net;

#if NET461_OR_GREATER || NETCOREAPP
using Microsoft.Extensions.Configuration;
#endif

namespace qckdev.Net.Http.Test.Common
{
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

#if NET461_OR_GREATER || NETCOREAPP

        var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .AddEnvironmentVariables();

        var config = builder.Build();

        return config.Get<Configuration.Settings>();

#else
            string fileName = "appsettings.json";
            string fileNameByEnv = $"appsettings.{environment}.json";
            var finalName
                = (string.IsNullOrEmpty(environment?.Trim()) || !System.IO.File.Exists(fileNameByEnv)) ?
                    fileName : fileNameByEnv;

            using (var reader = new System.IO.StreamReader(finalName))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration.Settings>(reader.ReadToEnd());
            }

#endif
        }

    }
}

using System.Net;
#if NO_SYNC 
using System.IO;
using Newtonsoft.Json;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace qckdev.Net.Http.Test
{
    static class Helpers
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty")]
        public static void SetDefaultSecurityProtocol()
        {
#if SET_SECURITY_PROTOCOL
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
            
#if NO_SYNC
            string fileName = "appsettings.json";
            string fileNameByEnv = $"appsettings.{environment}.json";
            var finalName 
                = (string.IsNullOrWhiteSpace(environment) || !System.IO.File.Exists(fileNameByEnv)) ?
                    fileName : fileNameByEnv;
            
            using (var reader = new StreamReader(finalName))
            {
                return JsonConvert.DeserializeObject<Configuration.Settings>(reader.ReadToEnd());
            }
#else
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            
            return config.Get<Configuration.Settings>();
#endif
        }

    }
}

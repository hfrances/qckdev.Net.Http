#if NO_HTTP
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="HttpClient"/>.
    /// </summary>
    public static partial class HttpClientExtensions
    {

        private static string JsonSerializeObject(object content)
        {
            return content != null ? JsonConvert.SerializeObject(content) : null;
        }

    }
}
#endif
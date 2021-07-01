#if NO_DYNAMIC
#else
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }


    }
}
#endif
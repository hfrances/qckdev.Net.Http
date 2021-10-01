#if NO_DYNAMIC
#else
using System;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
        }

        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
        }

        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
        }

#if NO_SYNC
#else

        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

#endif

    }
}
#endif
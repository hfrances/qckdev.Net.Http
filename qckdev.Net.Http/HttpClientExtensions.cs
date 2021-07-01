using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await Fetch<TResult, dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await Fetch<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null);
        }

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await Fetch<TResult, dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = (content != null ?
                            new StringContent(
                                content,
                                Encoding.UTF8, "application/json")
                            :
                            null)
            };

            using (request)
            {
                return await Fetch<TResult, TError>(client, request);
            }
        }

        public async static Task<dynamic> Fetch(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await Fetch<dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await Fetch<TResult, dynamic>(client, method, requestUri, content);
        }

        public async static Task<TResult> Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            using (request)
            {
                return await Fetch<TResult, TError>(client, request);
            }
        }

        private async static Task<TResult> Fetch<TResult, TError>(HttpClient client, HttpRequestMessage request)
        {

            using (var response = await client.SendAsync(request))
            {
                var jsonString = await response.Content?.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return
                        (jsonString == null) ? default :
                            JsonConvert.DeserializeObject<TResult>(jsonString);
                }
                else
                {
                    var error =
                        (jsonString == null) ? default :
                            JsonConvert.DeserializeObject<TError>(jsonString);

                    throw new FetchFailedException<TError>(response.StatusCode, response.ReasonPhrase, error);
                }
            }
        }

    }
}

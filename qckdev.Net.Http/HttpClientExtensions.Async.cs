using qckdev.Text.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null);
        }

        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = (content != null ?
                            new StringContent(
                                content,
                                Encoding.UTF8, Constants.MEDIATYPE_APPLICATIONJSON)
                            :
                            null)
            };

            using (request)
            {
                return await FetchAsync<TResult, TError>(client, request);
            }
        }

        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            using (request)
            {
                return await FetchAsync<TResult, TError>(client, request);
            }
        }

        private async static Task<TResult> FetchAsync<TResult, TError>(HttpClient client, HttpRequestMessage request)
        {

            using (var response = await client.SendAsync(request))
            {
                var jsonString = await response.Content?.ReadAsStringAsync();
                var isJson = 
                    (response.Content.Headers.ContentType?.MediaType ?? string.Empty)
                        .Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase);

                if (response.IsSuccessStatusCode)
                {
                    return
                        (jsonString == null || !isJson) ?
                            default :
                            JsonConvert.DeserializeObject<TResult>(jsonString)
                        ;
                }
                else if (typeof(TError) == typeof(object))
                {
                    var error =
                        (jsonString == null || !isJson) ?
                            default :
                            JsonConvert.DeserializeObject(jsonString)
                        ;

                    throw new FetchFailedException(
                        request.Method, request.RequestUri, response.StatusCode,
                        response.ReasonPhrase, error
                    );
                }
                else
                {
                    var error =
                        (jsonString == null || !isJson) ?
                            default :
                            JsonConvert.DeserializeObject<TError>(jsonString)
                        ;

                    throw new FetchFailedException<TError>(
                        request.Method, request.RequestUri, response.StatusCode,
                        response.ReasonPhrase, error
                    );
                }
            }
        }

    }
}

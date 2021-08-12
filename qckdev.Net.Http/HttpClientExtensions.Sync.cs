#if NO_SYNC
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        public static TResult Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return Fetch<TResult, object>(client, method, requestUri, content);
        }

        public static TResult Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return Fetch<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null);
        }

        public static TResult Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return Fetch<TResult, object>(client, method, requestUri, content);
        }

        public static TResult Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            var request = new HttpRequestMessageSync(method, requestUri)
            {
                Content = (content != null ?
                            new StringContentSync(
                                content,
                                Encoding.UTF8, Constants.MEDIATYPE_APPLICATIONJSON)
                            :
                            null)
            };

            using (request)
            {
                return Fetch<TResult, TError>(client, request);
            }
        }

        private static TResult Fetch<TResult, TError>(HttpClient client, HttpRequestMessageSync request)
        {
            var http = WebRequest.CreateHttp(new Uri(client.BaseAddress, request.RequestUri));
            HttpWebResponse response;

            http.Method = request.Method.Method;
            http.Headers.AddRange(request.Headers, client.DefaultRequestHeaders);
            http.SetContent(request);

            try
            {
                response = (HttpWebResponse)http.GetResponse();
            }
            catch (WebException ex) when (ex.Response != null)
            {
                response = (HttpWebResponse)ex.Response;
            }

            using (response)
            {
                var jsonString = response.GetContentAsString();
                var isJson =
                    (response.ContentType ?? string.Empty)
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Contains(Constants.MEDIATYPE_APPLICATIONJSON, StringComparer.OrdinalIgnoreCase);

                if (response.IsSuccessStatusCode())
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
                        response.StatusDescription, error
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
                        response.StatusDescription, error
                    );
                }
            }
        }

        private static void AddRange(this WebHeaderCollection collection, params IEnumerable<KeyValuePair<string, IEnumerable<string>>>[] headers)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> combinedHeaders = null;
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders;

            foreach (var header in headers)
            {
                if (combinedHeaders == null)
                {
                    combinedHeaders = header;
                }
                else
                {
                    combinedHeaders = combinedHeaders.Union(header);
                }
            }

            requestHeaders =
                combinedHeaders
                    .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new KeyValuePair<string, IEnumerable<string>>(
                            x.Key, x.Last().Value
                        ));
            foreach (var header in requestHeaders)
            {
                collection.Add(header.Key, string.Join(", ", header.Value));
            }
        }

        private static void SetContent(this WebRequest @this, HttpRequestMessageSync request)
        {
            @this.ContentType = request.Content?.Headers.ContentType.ToString();

            if (request.Content != null)
            {
                using (var reader = request.Content.ReadAsStream())
                {
                    @this.ContentLength = reader.Length;

                    var stream = @this.GetRequestStream();
                    reader.CopyTo(stream);
                    stream.Close();
                }
            }
        }

    }
}
#endif
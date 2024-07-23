#if NO_SYNC || NO_HTTP
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Dynamic;
using System.Text;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null, FetchOptions<TResult> options = null)
        {
            return Fetch<TResult>(client, method, requestUri, JsonSerializeObject(content), options);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null, FetchOptions<TResult, TError> options = null)
        {
            return Fetch<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null,
                options);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content, FetchOptions<TResult> options = null)
        {
            return Fetch<TResult, ExpandoObject>(client, method, requestUri, content, options);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content, FetchOptions<TResult, TError> options = null)
        {
#if NET5_0_OR_GREATER
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = (content != null ?
                            new StringContent(
                                content,
                                Encoding.UTF8, Constants.MEDIATYPE_APPLICATION_JSON)
                            :
                            null)
            };
#else
            var request = new HttpRequestMessageSync(method, requestUri)
            {
                Content = (content != null ?
                            new StringContentSync(
                                content,
                                Encoding.UTF8, Constants.MEDIATYPE_APPLICATION_JSON)
                            :
                            null)
            };
#endif

            using (request)
            {
                return Fetch<TResult, TError>(client, request, options);
            }
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="request">A <see cref="HttpRequestMessage"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(HttpClient client, HttpRequestMessage request, FetchOptions<TResult, TError> options = null)
        {
            try
            {
                using (var response = client.Send(request))
                {
                    return response.DeserializeContent<TResult, TError>(options);
                }
            }
            catch (FetchFailedException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new FetchFailedException<TError>(
                    request.Method.Method, new Uri(client.BaseAddress, request.RequestUri),
                    request.Headers.ToDictionary(x => x.Key, y => y.Value),
                    request.Content?.Headers.ContentType?.ToString(),
                    request.Content == null ? null : request.Content.ReadAsString(),
                    ex.StatusCode, ex.Message, default, ex
                );
            }
        }
#else
        private static TResult Fetch<TResult, TError>(HttpClient client, HttpRequestMessageSync request, FetchOptions<TResult, TError> options = null)
        {
            var uri = (client.BaseAddress == null ? request.RequestUri : new Uri(client.BaseAddress, request.RequestUri));
            var http = WebRequest.CreateHttp(uri);

            http.Method = request.Method.Method;
            http.Headers.AddRange(request.Headers, client.DefaultRequestHeaders);
            http.SetContent(request);

            try
            {
                return http.Fetch<TResult, TError>(options);
            }
            catch (FetchFailedException<TError> ex)
            {
                throw new FetchFailedException<TError>(
                    http.Method, new Uri(client.BaseAddress, request.RequestUri),
                    http.Headers.ToDictionary(),
                    request.Content?.Headers.ContentType?.ToString(),
                    request.GetContentAsString(),
                    ex.StatusCode, ex.Message, ex.Error, ex
                );
            }
        }

        private static void SetContent(this HttpWebRequest @this, HttpRequestMessageSync request)
        {
            @this.ContentType = request.Content?.Headers.ContentType?.ToString();

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

        private static string GetContentAsString(this HttpRequestMessageSync request)
        {
            string rdo;

            if (request.Content != null)
            {
                using (var stream = request.Content.ReadAsStream())
                {
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        rdo = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                rdo = null;
            }
            return rdo;
        }

#endif
    }
}
#endif
#if NO_SYNC
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.Dynamic;

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
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null, FetchOptions<TResult> options = null)
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
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
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
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content, FetchOptions<TResult, TError> options = null)
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
                return Fetch<TResult, TError>(client, request, options);
            }
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult>(this HttpWebRequest request, FetchOptions<TResult> options = null)
        {
            return Fetch<TResult, ExpandoObject>(request, options);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpWebRequest request, FetchOptions<TResult, TError> options = null)
        {

            try
            {
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex) when (ex.Response != null)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                using (response)
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
                var method = new HttpMethod(request.Method);

#if NET5
                throw new FetchFailedException<TError>(method, request.RequestUri, ex.StatusCode, ex.Message, default);
#else
                throw new FetchFailedException<TError>(method, request.RequestUri, null, ex.Message, default);
#endif
            }
        }


        private static TResult Fetch<TResult, TError>(HttpClient client, HttpRequestMessageSync request, FetchOptions<TResult, TError> options = null)
        {
            var http = WebRequest.CreateHttp(new Uri(client.BaseAddress, request.RequestUri));

            http.Method = request.Method.Method;
            http.Headers.AddRange(request.Headers, client.DefaultRequestHeaders);
            http.SetContent(request);

            return http.Fetch<TResult, TError>(options);
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
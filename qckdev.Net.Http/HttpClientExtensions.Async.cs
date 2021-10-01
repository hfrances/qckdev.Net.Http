using qckdev.Text.Json;
using System;
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

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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
        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A container for name/value tuples encoded using application/x-www-form-urlencoded content of the HTTP message.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await FetchAsync<TResult, object>(client, method, requestUri, content);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A container for name/value tuples encoded using application/x-www-form-urlencoded content of the HTTP message.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
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

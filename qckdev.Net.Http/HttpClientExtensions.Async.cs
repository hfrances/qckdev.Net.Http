using qckdev.Text.Json;
using System;
using System.Dynamic;
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
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, object content = null, FetchAsyncOptions<TResult> options = null)
        {
            return await FetchAsync<TResult, ExpandoObject>(client, method, requestUri, content, options);
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
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, object content = null, FetchAsyncOptions<TResult, TError> options = null)
        {
            return await FetchAsync<TResult, TError>(client, method, requestUri,
                content != null ? JsonConvert.SerializeObject(content) : null,
                options);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, string content, FetchAsyncOptions<TResult> options = null)
        {
            return await FetchAsync<TResult, ExpandoObject>(client, method, requestUri, content, options);
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
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, string content, FetchAsyncOptions<TResult, TError> options = null)
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
                return await FetchAsync<TResult, TError>(client, request, options);
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
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content, FetchAsyncOptions<TResult> options = null)
        {
            return await FetchAsync<TResult, ExpandoObject>(client, method, requestUri, content, options);
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
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content, FetchAsyncOptions<TResult, TError> options = null)
        {
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            using (request)
            {
                return await FetchAsync<TResult, TError>(client, request, options);
            }
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="request">A <see cref="HttpRequestMessage"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<TResult> FetchAsync<TResult>(this HttpClient client, HttpRequestMessage request, FetchAsyncOptions<TResult> options = null)
        {
            return await FetchAsync<TResult, ExpandoObject>(client, request, options);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
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
        public async static Task<TResult> FetchAsync<TResult, TError>(this HttpClient client, HttpRequestMessage request, FetchAsyncOptions<TResult, TError> options = null)
        {

            try
            {
                using (var response = await client.SendAsync(request))
                {
                    return await response.DeserializeContentAsync<TResult, TError>(options);
                }
            }
            catch (FetchFailedException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
#if NET5_0_OR_GREATER
                throw new FetchFailedException<TError>(request.Method, new Uri(client.BaseAddress, request.RequestUri), ex.StatusCode, ex.Message, default);
#else
                throw new FetchFailedException<TError>(request.Method, new Uri(client.BaseAddress, request.RequestUri), null, ex.Message, default);
#endif                
            }
        }

    }
}

#if NO_HTTP || NO_DYNAMIC
#else
using System;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpClientExtensions
    {

#if NO_ASYNC
#else

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
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
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
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
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A container for name/value tuples encoded using application/x-www-form-urlencoded content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public async static Task<dynamic> FetchAsync(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return await FetchAsync<ExpandoObject>(client, method, requestUri, content);
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

#endif

#if NO_SYNC
#else

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, object content = null)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, string content)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A container for name/value tuples encoded using application/x-www-form-urlencoded content of the HTTP message.</param>
        /// <returns>A <see cref="ExpandoObject"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static dynamic Fetch(this HttpClient client, HttpMethod method, string requestUri, FormUrlEncodedContent content)
        {
            return Fetch<ExpandoObject>(client, method, requestUri, content);
        }

#endif

    }
}
#endif
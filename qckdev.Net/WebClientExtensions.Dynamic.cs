#if NO_WEB
#else
using System;
using System.Net;

namespace qckdev.Net
{

    /// <summary>
    /// Provides extension methods for <see cref="WebClient"/>.
    /// </summary>
    public static partial class WebClientExtensions
    {

#if NO_DYNAMIC
        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>An object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
#if NET6_0_OR_GREATER
        [Obsolete("WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.", DiagnosticId = "SYSLIB0014")]
#endif
        public static object Fetch(this WebClient client, string method, string requestUri, object content = null, FetchOptions<object> options = null)
        {
            return Fetch<object>(client, method, requestUri, content, options);
        }
#else
        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A dynamic object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
#if NET6_0_OR_GREATER
        [Obsolete("WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.", DiagnosticId = "SYSLIB0014")]
#endif
        public static dynamic Fetch(this WebClient client, string method, string requestUri, object content = null, FetchOptions<System.Dynamic.ExpandoObject> options = null)
        {
            return Fetch<System.Dynamic.ExpandoObject, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
        }
#endif

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
#if NET6_0_OR_GREATER
        [Obsolete("WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.", DiagnosticId = "SYSLIB0014")]
#endif
        public static TResult Fetch<TResult>(this WebClient client, string method, string requestUri, object content = null, FetchOptions<TResult> options = null)
        {
#if NO_DYNAMIC
            return Fetch<TResult, object>(client, method, requestUri, content, options);
#else
            return Fetch<TResult, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
#endif
        }

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
#if NET6_0_OR_GREATER
        [Obsolete("WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.", DiagnosticId = "SYSLIB0014")]
#endif
        public static TResult Fetch<TResult>(this WebClient client, string method, string requestUri, string content, FetchOptions<TResult> options = null)
        {
#if NO_DYNAMIC
            return Fetch<TResult, object>(client, method, requestUri, content, options);
#else
            return Fetch<TResult, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
#endif            
        }

    }
}
#endif





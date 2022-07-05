#if NO_WEB || NO_DYNAMIC
#else
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpWebRequestExtensions
    {

#if NO_ASYNC
#else
        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a status code out of the range 200-299.
        /// </exception>
        public static Task<TResult> FetchAsync<TResult>(this HttpWebRequest request, FetchOptions<TResult> options = null)
        {
            return FetchAsync<TResult, ExpandoObject>(request, options);
        }
#endif

#if NO_SYNC
#else
        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a status code out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult>(this HttpWebRequest request, FetchOptions<TResult> options = null)
        {
            return Fetch<TResult, ExpandoObject>(request, options);
        }
#endif

    }
}
#endif
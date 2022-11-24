#if NO_WEB
#else
using System.Net;

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
        public static System.Threading.Tasks.Task<TResult> FetchAsync<TResult>(this HttpWebRequest request, FetchAsyncOptions<TResult> options = null)
        {
            return FetchAsync<TResult, System.Dynamic.ExpandoObject>(request, options);
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
#if NO_DYNAMIC
            return Fetch<TResult, object>(request, options);
#else
            return Fetch<TResult, System.Dynamic.ExpandoObject>(request, options);
#endif
        }

#endif

    }
}
#endif
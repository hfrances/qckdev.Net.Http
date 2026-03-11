#if NO_ASYNC || NO_WEB
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace qckdev.Net
{
    public static partial class HttpWebRequestExtensions
    {

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Content"/>.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a status code out of the range 200-299.
        /// </exception>
#if NET6_0_OR_GREATER
        [Obsolete("WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.", DiagnosticId = "SYSLIB0014")]
#endif
        public static async Task<TResult> FetchAsync<TResult, TError>(this HttpWebRequest request, FetchAsyncOptions<TResult, TError> options = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)await request.GetResponseAsync();
                }
                catch (WebException ex) when (ex.Response != null)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                using (response)
                {
                    return await response.DeserializeContentAsync<TResult, TError>(options, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw CreateException<TError>(request, ex);
            }
        }

        internal static async Task<string> GetContentAsStringAsync(this HttpWebRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string rdo;

            if (request.ContentLength > 0)
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        rdo = await reader.ReadToEndAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            else
            {
                rdo = null;
            }
            return rdo;
        }

    }
}
#endif





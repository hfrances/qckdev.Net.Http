#if NO_ASYNC || NO_WEB
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    public static partial class HttpWebRequestExtensions
    {

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a status code out of the range 200-299.
        /// </exception>
        public static async Task<TResult> FetchAsync<TResult, TError>(this HttpWebRequest request, FetchAsyncOptions<TResult, TError> options = null)
        {

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
                    return await response.DeserializeContentAsync<TResult, TError>(options);
                }
            }
            catch (FetchFailedException)
            {
                throw;
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)ex.Response;

                throw new FetchFailedException<TError>(request.Method, request.RequestUri, response.StatusCode, response.StatusDescription, default, ex);
            }
            catch (WebException ex)
            {
                throw new FetchFailedException<TError>(request.Method, request.RequestUri, null, ex.Message, default, ex);
            }
        }

    }
}
#endif
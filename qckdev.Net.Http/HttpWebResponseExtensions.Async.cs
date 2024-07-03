#if NO_ASYNC || NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{

    public static partial class HttpWebResponseExtensions
    {

        /// <summary>
        /// Parses the content of the HTTP response message into an instance of the type
        /// </summary>
        /// <typeparam name="TResult">The target type of the HTTP response message value.</typeparam>
        /// <typeparam name="TError">The target type of the HTTP response throwed <see cref="FetchFailedException"/> exception.</typeparam>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="options">Options for custom deseralizing.</param>
        /// <returns>A representation of the HTTP response message.</returns>
        /// <exception cref="FetchFailedException{TError}">Throws when some error occurred while get the HTTP response.</exception>
        public static async Task<TResult> DeserializeContentAsync<TResult, TError>(this HttpWebResponse response, FetchAsyncOptions<TResult, TError> options = null)
        {

            if (response.IsSuccessStatusCode())
            {
                return await DeserializationHelper.HandleResponseAsync(
                    response.IsContentType,
                    () => Task.Factory.StartNew(() => response.GetContentAsString()),
                    options?.OnDeserializeAsync
                );
            }
            else
            {
                var method = response.Method;
                var result = await DeserializationHelper.HandleErrorAsync(
                    response.IsContentType,
                    () => Task.Factory.StartNew(() => response.GetContentAsString()),
                    () => Task.Factory.StartNew(() => response.StatusDescription),
                    options?.OnDeserializeErrorAsync
                );
                throw new FetchFailedException<TError>(
                    method, response.ResponseUri,
                    null, null, null,
                    response.StatusCode, result.ReasonPhrase, result.ContentString, result.Content
                );
            }
        }

    }
}
#endif
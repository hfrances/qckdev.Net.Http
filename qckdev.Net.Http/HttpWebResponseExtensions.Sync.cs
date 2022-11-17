#if NO_SYNC || NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="HttpWebResponse"/>.
    /// </summary>
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
        public static TResult DeserializeContent<TResult, TError>(this HttpWebResponse response, FetchOptions<TResult, TError> options = null)
        {

            if (response.IsSuccessStatusCode())
            {
                return DeserializationHelper.HandleResponse(
                    response.IsContentType,
                    response.GetContentAsString,
                    options?.OnDeserialize
                );
            }
            else
            {
                var method = response.Method;
                var result = DeserializationHelper.HandleError(
                    response.IsContentType,
                    response.GetContentAsString,
                    () => response.StatusDescription,
                    options?.OnDeserializeError
                );
                throw new FetchFailedException<TError>(
                    method, response.ResponseUri, 
                    null, null, null,
                    response.StatusCode, result.ReasonPhrase, result.ErrorContent
                );
            }
        }

    }
}
#endif
#if NO_HTTP
#else
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{


    static class HttpResponseMessageExtensions
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
        public static async Task<TResult> DeserializeContentAsync<TResult, TError>(this HttpResponseMessage response, FetchAsyncOptions<TResult, TError> options)
        {
            var contentType = response.GetMediaType();

            if (response.IsSuccessStatusCode)
            {
                return await DeserializationHelper.HandleResponseAsync(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    response.Content.ReadAsStringAsync,
                    options?.OnDeserializeAsync
                );
            }
            else
            {
                var errorDetails = await DeserializationHelper.HandleErrorAsync(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    () => response.Content.ReadAsStringAsync(),
                    () => Task.FromResult(response.ReasonPhrase),
                    options?.OnDeserializeErrorAsync
                );

                throw await HttpRequestMessageHelper.CreateExceptionAsync(response.RequestMessage, response.StatusCode, errorDetails);
            }
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// Parses the content of the HTTP response message into an instance of the type
        /// </summary>
        /// <typeparam name="TResult">The target type of the HTTP response message value.</typeparam>
        /// <typeparam name="TError">The target type of the HTTP response throwed <see cref="FetchFailedException"/> exception.</typeparam>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="options">Options for custom deseralizing.</param>
        /// <returns>A representation of the HTTP response message.</returns>
        /// <exception cref="FetchFailedException{TError}">Throws when some error occurred while get the HTTP response.</exception>
        public static TResult DeserializeContent<TResult, TError>(this HttpResponseMessage response, FetchOptions<TResult, TError> options)
        {
            var contentType = response.GetMediaType();

            if (response.IsSuccessStatusCode)
            {
                return DeserializationHelper.HandleResponse(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    response.Content.ReadAsString,
                    options?.OnDeserialize
                );
            }
            else
            {
                var result = DeserializationHelper.HandleError(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    response.Content.ReadAsString,
                    () => response.ReasonPhrase,
                    options?.OnDeserializeError
                );

                throw HttpRequestMessageHelper.CreateException(response.RequestMessage, response.StatusCode, result);
            }
        }       

#endif

        static string GetMediaType(this HttpResponseMessage response)
        {
            if (response.Content?.Headers.ContentLength > 0)
            {
                return response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
#endif
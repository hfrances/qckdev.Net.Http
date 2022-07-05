#if NO_HTTP
#else
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var contentType = response.GetContentType();

            if (response.IsSuccessStatusCode)
            {
                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    var stringContent = await response.Content.ReadAsStringAsync();

                    if (options?.OnDeserializeAsync == null)
                    {
                        return await Task.FromResult(JsonConvert.DeserializeObject<TResult>(stringContent));
                    }
                    else
                    {
                        return await options.OnDeserializeAsync(stringContent);
                    }
                }
                else if (contentType.Equals(Constants.MEDIATYPE_TEXTPLAIN, StringComparison.OrdinalIgnoreCase))
                {
                    return (TResult)Convert.ChangeType(await response.Content.ReadAsStringAsync(), typeof(TResult));
                }
                else
                {
                    return default;
                }
            }
            else
            {
                var result = await DeserializationHelper.HandleErrorAsync(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    () => response.Content.ReadAsStringAsync(),
                    () => Task.FromResult(response.ReasonPhrase), 
                    options?.OnDeserializeErrorAsync
                );

                throw new FetchFailedException<TError>(response.RequestMessage.Method.Method, response.RequestMessage.RequestUri, response.StatusCode, result.ReasonPhrase, result.ErrorContent);
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
            var contentType = response.GetContentType();

            if (response.IsSuccessStatusCode)
            {
                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    string stringContent = response.Content.ReadAsString();

                    if (options?.OnDeserialize == null)
                    {
                        return JsonConvert.DeserializeObject<TResult>(stringContent);
                    }
                    else
                    {
                        return options.OnDeserialize(stringContent);
                    }
                }
                else if (contentType.Equals(Constants.MEDIATYPE_TEXTPLAIN, StringComparison.OrdinalIgnoreCase))
                {
                    return (TResult)Convert.ChangeType(response.Content.ReadAsString(), typeof(TResult));
                }
                else
                {
                    return default;
                }
            }
            else
            {
                var result = DeserializationHelper.HandleError(
                    x => contentType.Equals(x, StringComparison.OrdinalIgnoreCase),
                    () => response.Content.ReadAsString(),
                    () => response.ReasonPhrase,
                    options?.OnDeserializeError
                );

                throw new FetchFailedException<TError>(response.RequestMessage.Method.Method, response.RequestMessage.RequestUri, response.StatusCode, result.ReasonPhrase, result.ErrorContent);
            }
        }
#endif

        static string GetContentType(this HttpResponseMessage response)
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
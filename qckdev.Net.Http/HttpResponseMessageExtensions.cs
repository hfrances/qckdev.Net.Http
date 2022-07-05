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
                TError errorContent;
                string reasonPhrase;

                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    var stringContent = await response.Content.ReadAsStringAsync();

                    reasonPhrase = response.ReasonPhrase;
                    if (string.IsNullOrWhiteSpace(stringContent))
                    {
                        errorContent = default;
                    }
                    if (options?.OnDeserializeErrorAsync == null)
                    {
                        errorContent = await Task.FromResult(JsonConvert.DeserializeObject<TError>(stringContent));
                    }
                    else
                    {
                        errorContent = await options.OnDeserializeErrorAsync(stringContent);
                    }
                }
                else if (contentType.Equals(Constants.MEDIATYPE_TEXTPLAIN, StringComparison.OrdinalIgnoreCase))
                {
                    reasonPhrase = await response.Content.ReadAsStringAsync();
                    errorContent = default;
                }
                else
                {
                    reasonPhrase = response.ReasonPhrase;
                    errorContent = default;
                }
                throw new FetchFailedException<TError>(response.RequestMessage.Method.Method, response.RequestMessage.RequestUri, response.StatusCode, reasonPhrase, errorContent);
            }
        }

#if NET5_0_OR_GREATER

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
                TError errorContent;
                string reasonPhrase;

                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    string stringContent = response.Content.ReadAsString();

                    reasonPhrase = response.ReasonPhrase;
                    if (string.IsNullOrWhiteSpace(stringContent))
                    {
                        errorContent = default;
                    }
                    if (options?.OnDeserializeError == null)
                    {
                        errorContent = JsonConvert.DeserializeObject<TError>(stringContent);
                    }
                    else
                    {
                        errorContent = options.OnDeserializeError(stringContent);
                    }
                }
                else if (contentType.Equals(Constants.MEDIATYPE_TEXTPLAIN, StringComparison.OrdinalIgnoreCase))
                {
                    reasonPhrase = response.Content.ReadAsString();
                    errorContent = default;
                }
                else
                {
                    reasonPhrase = response.ReasonPhrase;
                    errorContent = default;
                }
                throw new FetchFailedException<TError>(response.RequestMessage.Method.Method, response.RequestMessage.RequestUri, response.StatusCode, reasonPhrase, errorContent);
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
#if NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="WebClient"/>.
    /// </summary>
    public static class WebClientExtensions
    {

        public static TResult Fetch<TResult, TError>(this WebClient client, string method, string requestUri, object content = null, FetchOptions<TResult, TError> options = null)
        {
            string contentString;

            if (content == null)
            {
                contentString = null;
            }
            else
            {
                contentString = JsonConvert.SerializeObject<object>(content);
            }
            return Fetch<TResult, TError>(client, method, requestUri, contentString, options);
        }

        public static TResult Fetch<TResult, TError>(this WebClient client, string method, string requestUri, string content, FetchOptions<TResult, TError> options = null)
        {
            var fullUri = (string.IsNullOrEmpty(client.BaseAddress) ? new Uri(requestUri) : new Uri(new Uri(client.BaseAddress), requestUri));

            try
            {
                string rdo;

                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    rdo = client.DownloadString(fullUri);
                }
                else
                {
                    if (string.IsNullOrEmpty(client.Headers[HttpRequestHeader.ContentType]))
                    {
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    }
                    if (string.IsNullOrEmpty(client.Headers["charset"]))
                    {
                        client.Headers.Add("charset", "'utf-8'");
                    }
                    rdo = client.UploadString(fullUri, method, content ?? string.Empty);
                }

                if (string.IsNullOrEmpty(rdo) || rdo.Trim() == string.Empty)
                {
                    return default;
                }
                else if (options?.OnDeserialize == null)
                {
                    return JsonConvert.DeserializeObject<TResult>(rdo);
                }
                else
                {
                    return options.OnDeserialize(rdo);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    var result = DeserializationHelper.HandleError(
                        httpResponse.IsContentType,
                        httpResponse.GetContentAsString,
                        () => httpResponse.StatusDescription,
                        options?.OnDeserializeError
                    );

                    throw new FetchFailedException<TError>(method, httpResponse.ResponseUri, httpResponse.StatusCode, result.ReasonPhrase, result.ErrorContent, ex);
                }
                else
                {
                    throw new FetchFailedException<TError>(method, ex.Response.ResponseUri, null, ex.Message, default, ex);
                }
            }
        }

    }
}
#endif
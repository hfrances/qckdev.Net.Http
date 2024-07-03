#if NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="WebClient"/>.
    /// </summary>
    public static partial class WebClientExtensions
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
            IDictionary<string, IEnumerable<string>> requestHeaders = null;
            var fullUri = (string.IsNullOrEmpty(client.BaseAddress) ? new Uri(requestUri) : new Uri(new Uri(client.BaseAddress), requestUri));

            try
            {
                string response;
                string mediaTypeResponse;

                if (string.IsNullOrEmpty(client.Headers[HttpRequestHeader.ContentType]))
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                }
                if (string.IsNullOrEmpty(client.Headers["charset"]))
                {
                    client.Headers.Add("charset", "utf-8");
                }

                requestHeaders = client.Headers.ToDictionary(); // Copy headers because they are replaced after fetch .

                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    response = client.DownloadString(fullUri);
                }
                else
                {
                    response = client.UploadString(fullUri, method, content ?? string.Empty);
                }
                mediaTypeResponse = GetResponseMediaType(client);
                return DeserializationHelper.HandleResponse(
                    x => mediaTypeResponse?.Equals(x, StringComparison.OrdinalIgnoreCase) == true,
                    () => response,
                    options?.OnDeserialize
                );
            }
            catch (WebException ex)
            {
                IEnumerable<string> mediaType = null, charset = null;
                string contentType;

                requestHeaders?.TryGetValue("content-type", out mediaType);
                requestHeaders?.TryGetValue("charset", out charset);

                contentType = $"{mediaType?.FirstOrDefault()}{(mediaType?.Any() == true ? "; " : "")}{(charset?.Any() == true ? $"charset={charset.First()}" : "")}";
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    var result = DeserializationHelper.HandleError(
                        httpResponse.IsContentType,
                        httpResponse.GetContentAsString,
                        () => httpResponse.StatusDescription,
                        options?.OnDeserializeError
                    );

                    throw new FetchFailedException<TError>(
                        method, httpResponse.ResponseUri,
                        client.Headers.ToDictionary(),
                        contentType, content,
                        httpResponse.StatusCode, result.ReasonPhrase, result.ContentString, result.Content, ex
                    );
                }
                else
                {
                    throw new FetchFailedException<TError>(
                        method, fullUri,
                        client.Headers.ToDictionary(),
                        contentType, content,
                        null, ex.Message, null, default, ex
                    );
                }
            }
        }

        private static string GetResponseMediaType(WebClient client)
        {
            string result = null;
            var responseHeaders = client.ResponseHeaders?.ToDictionary();
            IEnumerable<string> contentType = null;

            responseHeaders.TryGetValue("content-type", out contentType);
            if (contentType != null)
            {
                result =
                    contentType
                        .SelectMany(x => x
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        .FirstOrDefault()?
                        .Trim();
            }
            return result;
        }

    }
}
#endif
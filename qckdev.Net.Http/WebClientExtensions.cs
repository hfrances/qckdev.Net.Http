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

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">Contents encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
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

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
        /// <param name="client">The <see cref="WebClient"/> which sends the request.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="content">A string encoded using application/json content of the HTTP message.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned an unsuccessful status code.
        /// </exception>
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
                        contentType,
                        content,
                        httpResponse.StatusCode, result.ReasonPhrase, result.ErrorContent, ex
                    );
                }
                else
                {
                    throw new FetchFailedException<TError>(
                        method, fullUri,
                        client.Headers.ToDictionary(),
                        contentType,
                        content,
                        null, ex.Message, default, ex
                    );
                }
            }
        }

        /// <summary>
        /// Gets the media type from the WebClient response headers.
        /// </summary>
        /// <param name="client">The WebClient containing the response headers.</param>
        /// <returns>The media type of the response or null if not available.</returns>
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
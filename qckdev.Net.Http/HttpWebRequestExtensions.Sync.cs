#if NO_SYNC || NO_WEB
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
        /// The request returned a <see cref="HttpResponseMessage.StatusCode"/> out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpWebRequest request, FetchOptions<TResult, TError> options = null)
        {

            try
            {
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex) when (ex.Response != null)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                using (response)
                {
                    return response.DeserializeContent<TResult, TError>(options);
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

        public static void SetContent(this HttpWebRequest request, object content)
        {
            var contentString =  qckdev.Text.Json.JsonConvert.SerializeObject<object>(content);

            SetContent(request, contentString);
        }

        public static void SetContent(this HttpWebRequest request, string content)
        {
            SetContent(request, content, System.Text.Encoding.UTF8);
        }

        public static void SetContent(this HttpWebRequest request, string content, System.Text.Encoding encoding)
        {
            request.ContentType = $"{Constants.MEDIATYPE_APPLICATIONJSON}; charset={encoding.EncodingName}";

            if (content != null)
            {
                var contentArray = encoding.GetBytes(content);

                request.ContentLength = contentArray.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(contentArray, 0, contentArray.Length);
                    stream.Close();
                }
            }
        }

        internal static void AddRange(this WebHeaderCollection collection, params IEnumerable<KeyValuePair<string, IEnumerable<string>>>[] headers)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> combinedHeaders = null;
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders;

            foreach (var header in headers)
            {
                if (combinedHeaders == null)
                {
                    combinedHeaders = header;
                }
                else
                {
                    combinedHeaders = combinedHeaders.Union(header);
                }
            }

            requestHeaders =
                combinedHeaders
                    .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new KeyValuePair<string, IEnumerable<string>>(
                            x.Key, x.Last().Value
                        ));
            foreach (var header in requestHeaders)
            {
                collection.Add(header.Key, string.Join(", ", header.Value.ToArray()));
            }
        }

    }
}
#endif
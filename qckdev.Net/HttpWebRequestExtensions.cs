#if NO_WEB
#else
using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="HttpWebRequest"/>.
    /// </summary>
    public static partial class HttpWebRequestExtensions
    {

        /// <summary>
        /// Adds a range of headers to a WebHeaderCollection.
        /// </summary>
        /// <param name="collection">The WebHeaderCollection to add headers to.</param>
        /// <param name="headers">Arrays of key-value pairs representing headers to add.</param>
        public static void AddRange(this WebHeaderCollection collection, params IEnumerable<KeyValuePair<string, IEnumerable<string>>>[] headers)
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

        /// <summary>
        /// Converts a WebHeaderCollection to a dictionary.
        /// </summary>
        /// <param name="collection">The WebHeaderCollection to convert.</param>
        /// <returns>A dictionary containing the headers from the collection.</returns>
        public static IDictionary<string, IEnumerable<string>> ToDictionary(this WebHeaderCollection collection)
        {
            var rdo = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in collection.AllKeys)
            {
                rdo.Add(key, new[] { collection[key] });
            }
            return rdo;
        }

        /// <summary>
        /// Creates an appropriate exception for the specified HttpWebRequest.
        /// </summary>
        /// <typeparam name="TError">The type of error to include in the exception.</typeparam>
        /// <param name="request">The HttpWebRequest that caused the exception.</param>
        /// <param name="ex">The original exception.</param>
        /// <returns>A new exception with additional context from the request.</returns>
        private static Exception CreateException<TError>(HttpWebRequest request, Exception ex)
        {
            Exception rdo;

            if (ex is WebException wex)
            {
                HttpStatusCode? statusCode;
                string statusDescription;

                if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = (HttpWebResponse)wex.Response;

                    statusCode = response.StatusCode;
                    statusDescription = response.StatusDescription;
                }
                else
                {
                    statusCode = null;
                    statusDescription = ex.Message;
                }
                rdo = new FetchFailedException<TError>(
                        request.Method, request.RequestUri,
                        request.Headers.ToDictionary(),
                        null, null,
                        statusCode, statusDescription, default, ex
                    );
            }
            else
            {
                rdo = ex;
            }
            return rdo;
        }

    }
}
#endif
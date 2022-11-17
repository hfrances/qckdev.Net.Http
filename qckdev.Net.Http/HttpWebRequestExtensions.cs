#if NO_WEB
#else
using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="HttpWebRequestExtensions"/>.
    /// </summary>
    public static partial class HttpWebRequestExtensions
    {

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

        internal static IDictionary<string, IEnumerable<string>> ToDictionary(this WebHeaderCollection collection)
        {
            var rdo = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in collection.AllKeys)
            {
                rdo.Add(key, new[] { collection[key] });
            }
            return rdo;
        }


    }
}
#endif
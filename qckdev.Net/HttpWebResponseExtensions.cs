#if NO_WEB
#else
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
        /// Returns the content of a HTTP response message as a string.
        /// </summary>
        /// <param name="response">The HTTP web response to read content from.</param>
        /// <returns>A string containing the response content.</returns>
        public static string GetContentAsString(this HttpWebResponse response)
        {
            string rdo;

            using (var responseStream = response.GetResponseStream())
            {
                Encoding encoding;

                if (string.IsNullOrEmpty(response.CharacterSet) || response.CharacterSet.Trim() == string.Empty)
                {
                    encoding = Encoding.Default;
                }
                else
                {
                    encoding = Encoding.GetEncoding(response.CharacterSet);
                }
                using (var reader = new System.IO.StreamReader(responseStream, encoding))
                {
                    rdo = reader.ReadToEnd();
                }
            }
            return rdo;
        }

        /// <summary>
        /// Gets a value that indicates whether the HTTP response was successful.
        /// </summary>
        /// <param name="response">The HTTP web response to check.</param>
        /// <returns>
        /// A value that indicates whether the HTTP response was successful. 
        /// true if <see cref="HttpStatusCode"/> is in the Successful range (200-299); otherwise, false.
        /// </returns>
        public static bool IsSuccessStatusCode(this HttpWebResponse response)
        {
            var statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode < 300;
        }

        /// <summary>
        /// Determines if the HTTP response's content type matches the specified media type.
        /// </summary>
        /// <param name="response">The HTTP web response to check.</param>
        /// <param name="mediaType">The media type to check for.</param>
        /// <returns>true if the response content type matches the specified media type; otherwise, false.</returns>
        internal static bool IsContentType(this HttpWebResponse response, string mediaType)
        {
            if (response.ContentLength != 0)
            {
                return response.ContentType?
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Contains(mediaType, StringComparer.OrdinalIgnoreCase)
                    ?? false;
            }
            else
            {
                return false;
            }
        }

    }
}
#endif
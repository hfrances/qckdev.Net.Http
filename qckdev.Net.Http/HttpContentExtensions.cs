#if NO_HTTP
#else
using qckdev.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    static class HttpContentExtensions
    {

#if NET5_0_OR_GREATER

        /// <summary>
        /// Reads the content as a string using the character set specified in the content's headers or the default encoding.
        /// </summary>
        /// <param name="content">The HTTP content to read.</param>
        /// <returns>A string containing the content.</returns>
        public static string ReadAsString(this HttpContent content)
        {
            using (var stream = content.ReadAsStream())
            {
                var charset = content.Headers.ContentType?.CharSet;
                Encoding encoding;

                if (string.IsNullOrWhiteSpace(charset))
                {
                    encoding = Encoding.Default;
                }
                else
                {
                    encoding = Encoding.GetEncoding(charset);
                }

                using (var reader = new System.IO.StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
#endif

    }
}
#endif




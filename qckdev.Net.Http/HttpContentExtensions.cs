#if NO_HTTP
#else
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
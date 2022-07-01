#if NO_HTTP
#else
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System;

namespace qckdev.Net.Http
{
    sealed class StringContentSync : ByteArrayContentSync
    {

        public StringContentSync(string content, Encoding encoding, string mediaType)
            : base(GetContentByteArray(content, encoding))
        {

            Headers.ContentType = 
                new MediaTypeHeaderValue(mediaType ?? Constants.MEDIATYPE_TEXTPLAIN)
                {
                    CharSet = (encoding == null) ? 
                        DefaultStringEncoding.WebName : encoding.WebName
                };
        }

        private static byte[] GetContentByteArray(string content, Encoding encoding)
        {

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            else if (encoding == null)
            {
                encoding = DefaultStringEncoding;
            }
            return encoding.GetBytes(content);
        }
    }
}
#endif
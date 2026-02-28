#if NO_HTTP
#else
using qckdev.Net;
using System;
using System.IO;
using System.Net.Http;

namespace qckdev.Net.Http
{

    [Obsolete("HttpContent it not compatible with sync processes.")]
    class HttpStreamContentSync : HttpContentSync
    {

        HttpContent Content { get; }

        public HttpStreamContentSync(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.Content = content;
        }

        private MemoryStream CreateMemoryStream()
        {
            var stream = new MemoryStream();

            Content.CopyToAsync(stream).Wait();
            return stream;
        }

        protected override Stream CreateContentReadStream()
        {
            return CreateMemoryStream();
        }

    }
}
#endif




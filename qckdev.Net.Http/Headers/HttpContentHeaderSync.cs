#if NO_HTTP
#else
using System.Net.Http.Headers;

namespace qckdev.Net.Http.Headers
{
    sealed class HttpContentHeaderSync : HttpHeaders
    {

        public MediaTypeHeaderValue ContentType { get; set; }

        public HttpContentHeaderSync()
        {

        }

    }
}
#endif
#if NO_HTTP
#else
using System.Net.Http.Headers;

namespace qckdev.Net.Http.Headers
{
    sealed class HttpRequestHeaderSync : HttpHeaders
    {

        public HttpRequestHeaderSync()
        {

        }

    }
}
#endif
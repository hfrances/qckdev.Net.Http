#if NO_SYNC || NO_HTTP
#else
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace qckdev.Net.Http
{
    sealed class HttpRequestMessageSync : IDisposable
    {

#if NETSTANDARD1_2
        public HttpHeaders Headers { get; }
#else
        public HttpRequestHeaders Headers { get; }
#endif
        public HttpMethod Method { get; set; }
        public Uri RequestUri { get; set; }
        public HttpContentSync Content { get; set; }

        public HttpRequestMessageSync()
            : this(HttpMethod.Get, (Uri)null)
        { }

        public HttpRequestMessageSync(HttpMethod method, string requestUri)
            : this(method, new Uri(requestUri, UriKind.RelativeOrAbsolute))
        { }

        public HttpRequestMessageSync(HttpMethod method, Uri requestUri)
        {
#if NETSTANDARD1_2
            this.Headers = new Headers.HttpRequestHeaderSync();
#else
            this.Headers = (HttpRequestHeaders)Activator.CreateInstance(
                typeof(HttpRequestHeaders), nonPublic: true);
#endif
            this.Method = method;
            this.RequestUri = requestUri;
        }

        public void Dispose()
        {
            this.Content?.Dispose();
        }
    }
}
#endif
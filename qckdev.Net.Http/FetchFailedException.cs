using System;
using System.Net;
using System.Net.Http;

namespace qckdev.Net.Http
{

    public class FetchFailedException : HttpRequestException
    {

        public HttpMethod Method { get; }
        public Uri RequestUri { get; set; }
        public HttpStatusCode StatusCode { get; }

        public object Error { get; }

        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, string message, object error) 
            : base(message)
        {
            this.Method = method;
            this.RequestUri = requestUri;
            this.StatusCode = statusCode;
            this.Error = error;
        }

        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, string message, object error, Exception inner) 
            : base(message, inner)
        {
            this.Method = method;
            this.RequestUri = requestUri;
            this.StatusCode = statusCode;
            this.Error = error;
        }

    }
}

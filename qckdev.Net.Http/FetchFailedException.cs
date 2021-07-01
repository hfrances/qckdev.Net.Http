using System;
using System.Net;
using System.Net.Http;

namespace qckdev.Net.Http
{

    public class FetchFailedException : HttpRequestException
    {

        public HttpStatusCode StatusCode { get; }

        public FetchFailedException(HttpStatusCode statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public FetchFailedException(HttpStatusCode statusCode, string message, Exception inner) : base(message, inner)
        {
            this.StatusCode = statusCode;
        }

    }
}

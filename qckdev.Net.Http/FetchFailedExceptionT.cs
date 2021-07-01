using System;
using System.Net;
using System.Net.Http;

namespace qckdev.Net.Http
{

    public class FetchFailedException<TError> : FetchFailedException
    {

        public new TError Error { get; }
        
        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, string message, TError error) 
            : base(method, requestUri, statusCode, message, error)
        {
            this.Error = error;
        }

        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, TError error, string message, Exception inner) 
            : base(method, requestUri, statusCode, message, error, inner)
        {
            this.Error = error;
        }

    }
}

using System;
using System.Net;
using System.Net.Http;

namespace qckdev.Net.Http
{

    public class FetchFailedException<TError> : FetchFailedException
    {

        public TError Error { get; }

        public FetchFailedException(HttpStatusCode statusCode, string message, TError error) : base(statusCode, message)
        {
            this.Error = error;
        }

        public FetchFailedException(HttpStatusCode statusCode, TError error, string message, Exception inner) : base(statusCode, message, inner)
        {
            this.Error = error;
        }

    }
}

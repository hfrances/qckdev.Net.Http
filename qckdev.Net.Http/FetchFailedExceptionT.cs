using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace qckdev.Net.Http
{

    /// <summary>
    /// A base class for exceptions thrown by the <see cref="HttpClient"/> Fetch and FetchAsync methods.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Scope = "type", Target = "~T:qckdev.Net.Http.FetchFailedException`1")]
    public class FetchFailedException<TError> : FetchFailedException
    {

        /// <summary>
        /// Gets a <typeparamref name="TError"/> object with the content returned by the request.
        /// </summary>
        public new TError Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException{TError}"/> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="error">A <typeparamref name="TError"/> object with the content returned by the request.</param>
        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, string message, TError error) 
            : base(method, requestUri, statusCode, message, error)
        {
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException{TError}"/> class with a specific message that describes the current exception and an inner exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="error">A <typeparamref name="TError"/> object with the content returned by the request.</param>
        /// <param name="inner">The inner exception.</param>
        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode statusCode, TError error, string message, Exception inner) 
            : base(method, requestUri, statusCode, message, error, inner)
        {
            this.Error = error;
        }

    }
}

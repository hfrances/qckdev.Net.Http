using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

namespace qckdev.Net.Http
{

    /// <summary>
    /// A base class for exceptions thrown by the <see cref="HttpClient"/> Fetch and FetchAsync methods.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Scope = "type", Target = "~T:qckdev.Net.Http.FetchFailedException")]
    public class FetchFailedException : HttpRequestException
    {

        /// <summary>
        /// Gets the HTTP method used in the request.
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Gets a string that represents the request <see cref="System.Uri"/>.
        /// </summary>
        public Uri RequestUri { get; set; }

#if NET5
        // Included inf HttpRequestException
#else

        /// <summary>
        /// Gets the HTTP status code to be returned with the exception.
        /// </summary>
        /// <Returns>
        ///  An HTTP status code if the exception represents a non-successful result, otherwise null.
        /// </Returns>
        public HttpStatusCode? StatusCode { get; }

#endif

        /// <summary>
        /// Gets the content returned by the request.
        /// </summary>
        public object Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException"/> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="error">Content returned by the request.</param>
        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode? statusCode, string message, object error)
#if NET5
            : base(message, null, statusCode)
        {
#else
            : base(message)
        {
            this.StatusCode = statusCode;
#endif
            this.Method = method;
            this.RequestUri = requestUri;
            this.Error = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException"/> class with a specific message that describes the current exception and an inner exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="error">Content returned by the request.</param>
        /// <param name="inner">The inner exception.</param>
        public FetchFailedException(HttpMethod method, Uri requestUri, HttpStatusCode? statusCode, string message, object error, Exception inner)
#if NET5
            : base(message, inner, statusCode)
        {
#else
            :base(message, inner)
        {
            this.StatusCode = statusCode;
#endif
            this.Method = method;
            this.RequestUri = requestUri;
            this.Error = error;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace qckdev.Net.Http
{

    /// <summary>
    /// A base class for exceptions thrown by the Fetch and FetchAsync methods.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Scope = "type", Target = "~T:qckdev.Net.Http.FetchFailedException`1")]
    public class FetchFailedException<TError> : FetchFailedException
    {

        /// <summary>
        /// Gets a <typeparamref name="TError"/> object with the content returned by the request.
        /// </summary>
        public new TError Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException{TError}"/> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="requestHeaders">A list witch the headers of the request.</param>
        /// <param name="requestContentType">The content type of the request.</param>
        /// <param name="requestContent">The request body in string format.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="contentString">A string with the content returned by the request.</param>
        /// <param name="content">A <typeparamref name="TError"/> object with the content returned by the request.</param>
        public FetchFailedException(string method, Uri requestUri, IDictionary<string, IEnumerable<string>> requestHeaders, string requestContentType, string requestContent, HttpStatusCode? statusCode, string message, string contentString, TError content) 
            : base(method, requestUri, requestHeaders, requestContentType, requestContent, statusCode, message, contentString, content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchFailedException{TError}"/> class with a specific message that describes the current exception and an inner exception.
        /// </summary>
        /// <param name="method">The HTTP method used in the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="System.Uri"/>.</param>
        /// <param name="requestHeaders">A list witch the headers of the request.</param>
        /// <param name="requestContentType">The content type of the request.</param>
        /// <param name="requestContent">The request body in string format.</param>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="contentString">A string with the content returned by the request.</param>
        /// <param name="content">A <typeparamref name="TError"/> object with the content returned by the request.</param>
        /// <param name="inner">The inner exception.</param>
        public FetchFailedException(string method, Uri requestUri, IDictionary<string, IEnumerable<string>> requestHeaders, string requestContentType, string requestContent, HttpStatusCode? statusCode, string message, string contentString, TError content, Exception inner) 
            : base(method, requestUri, requestHeaders, requestContentType, requestContent, statusCode, message, contentString, content, inner)
        {
            this.Content = content;
        }

    }
}

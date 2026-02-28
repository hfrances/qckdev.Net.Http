#if NO_HTTP
#else
using qckdev.Net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    static class HttpRequestMessageHelper
    {

        public static Task<FetchFailedException<TError>> CreateExceptionAsync<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, DeserializationHelper.ErrorHandleResponse<TError> errorDetails, Exception innerException = null)
            => CreateExceptionAsync(request, statusCode, errorDetails.ReasonPhrase, errorDetails.ContentString, errorDetails.Content, innerException);

        public static async Task<FetchFailedException<TError>> CreateExceptionAsync<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, string message, string contentString, TError content, Exception innerException = null)
        {
            FetchFailedException<TError> rdo;
            var requestContent = request.Content;
            string requestStringContent;
            bool stringContentNotSupported;

            try
            {
                requestStringContent = (requestContent == null ? null : await requestContent.ReadAsStringAsync());
                stringContentNotSupported = false;
            }
            catch (ObjectDisposedException)
            {
                // NET Framework 4.6.1 (and maybe others) cannot retrieve the request content.
                requestStringContent = null;
                stringContentNotSupported = true;
            }

            rdo = new FetchFailedException<TError>(
                request.Method.Method, request.RequestUri,
                request.Headers.ToDictionary(x => x.Key, y => y.Value),
                requestContent?.Headers.ContentType?.ToString(),
                requestStringContent,
                statusCode, message,
                contentString, content, 
                innerException);

            if (stringContentNotSupported)
            {
                rdo.SetRequestContentNotSupported();
            }
            return rdo;
        }

#if NET5_0_OR_GREATER

        public static FetchFailedException<TError> CreateException<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, DeserializationHelper.ErrorHandleResponse<TError> errorDetails, Exception innerException = null)
            => CreateException(request, statusCode, errorDetails.ReasonPhrase, errorDetails.ContentString, errorDetails.Content, innerException);

        public static FetchFailedException<TError> CreateException<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, string message, string contentString, TError content, Exception innerException = null)
        {
            FetchFailedException<TError> rdo;
            var requestContent = request.Content;
            string requestStringContent;
            bool stringContentNotSupported;

            try
            {
                requestStringContent = (requestContent == null ? null : requestContent.ReadAsString());
                stringContentNotSupported = false;
            }
            catch (ObjectDisposedException)
            {
                requestStringContent = null;
                stringContentNotSupported = true;
            }

            rdo = new FetchFailedException<TError>(
                request.Method.Method, request.RequestUri,
                request.Headers.ToDictionary(x => x.Key, y => y.Value),
                requestContent?.Headers.ContentType?.ToString(),
                requestStringContent,
                statusCode, message,
                contentString, content,
                innerException);

            if (stringContentNotSupported)
            {
                rdo.SetRequestContentNotSupported();
            }
            return rdo;
        }

#endif

    }
}
#endif

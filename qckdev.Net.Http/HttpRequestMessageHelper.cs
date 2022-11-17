#if NO_HTTP
#else
using System;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    static class HttpRequestMessageHelper
    {


        public static Task<FetchFailedException<TError>> CreateExceptionAsync<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, DeserializationHelper.ErrorHandleResponse<TError> errorDetails, Exception innerException = null)
            => CreateExceptionAsync(request, statusCode, errorDetails.ReasonPhrase, errorDetails.ErrorContent, innerException);

        public static async Task<FetchFailedException<TError>> CreateExceptionAsync<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, string message, TError error, Exception innerException = null)
        {
            FetchFailedException<TError> rdo;
            var content = request.Content;
            string stringContent;
            bool stringContentNotSupported;

            try
            {
                stringContent = (content == null ? null : await content.ReadAsStringAsync());
                stringContentNotSupported = false;
            }
            catch (ObjectDisposedException)
            {
                // NET Framework 4.6.1 (and maybe others) cannot retrieve the request content.
                stringContent = null;
                stringContentNotSupported = true;
            }

            rdo = new FetchFailedException<TError>(
                request.Method.Method, request.RequestUri,
                request.Headers.ToDictionary(x => x.Key, y => y.Value),
                content?.Headers.ContentType?.ToString(),
                stringContent,
                statusCode, message, error, innerException
            );
            if (stringContentNotSupported)
            {
                rdo.SetRequestContentNotSupported();
            }
            return rdo;
        }

#if NET5_0_OR_GREATER

        public static FetchFailedException<TError> CreateException<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, DeserializationHelper.ErrorHandleResponse<TError> errorDetails, Exception innerException = null)
            => CreateException(request, statusCode, errorDetails.ReasonPhrase, errorDetails.ErrorContent, innerException);

        public static FetchFailedException<TError> CreateException<TError>(HttpRequestMessage request, HttpStatusCode? statusCode, string message, TError error, Exception innerException = null)
        {
            FetchFailedException<TError> rdo;
            var content = request.Content;
            string stringContent;
            bool stringContentNotSupported;

            try
            {
                stringContent = (content == null ? null : content.ReadAsString());
                stringContentNotSupported = false;
            }
            catch (ObjectDisposedException)
            {
                stringContent = null;
                stringContentNotSupported = true;
            }

            rdo = new FetchFailedException<TError>(
                request.Method.Method, request.RequestUri,
                request.Headers.ToDictionary(x => x.Key, y => y.Value),
                content?.Headers.ContentType?.ToString(),
                stringContent,
                statusCode, message, error, innerException
            );
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
#if NO_SYNC || NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="HttpWebResponse"/>.
    /// </summary>
    public static class HttpWebResponseExtensions
    {

        /// <summary>
        /// Parses the content of the HTTP response message into an instance of the type
        /// </summary>
        /// <typeparam name="TResult">The target type of the HTTP response message value.</typeparam>
        /// <typeparam name="TError">The target type of the HTTP response throwed <see cref="FetchFailedException"/> exception.</typeparam>
        /// <param name="response">The HTTP response object.</param>
        /// <param name="options">Options for custom deseralizing.</param>
        /// <returns>A representation of the HTTP response message.</returns>
        /// <exception cref="FetchFailedException{TError}">Throws when some error occurred while get the HTTP response.</exception>
        public static TResult DeserializeContent<TResult, TError>(this HttpWebResponse response, FetchOptions<TResult, TError> options = null)
        {

            if (response.IsSuccessStatusCode())
            {
                if (response.IsContentType(Constants.MEDIATYPE_APPLICATIONJSON))
                {
                    var stringContent = response.GetContentAsString();

                    if (options?.OnDeserialize == null)
                    {
                        return JsonConvert.DeserializeObject<TResult>(stringContent);
                    }
                    else
                    {
                        return options.OnDeserialize(stringContent);
                    }
                }
                else if (response.IsContentType(Constants.MEDIATYPE_TEXTPLAIN))
                {
                    return (TResult)Convert.ChangeType(response.GetContentAsString(), typeof(TResult));
                }
                else
                {
                    return default;
                }
            }
            else
            {
                var method = response.Method;
                var result = DeserializationHelper.HandleError(
                    response.IsContentType, 
                    response.GetContentAsString,
                    () => response.StatusDescription,
                    options?.OnDeserializeError
                );
                throw new FetchFailedException<TError>(method, response.ResponseUri, response.StatusCode, result.ReasonPhrase, result.ErrorContent);
            }
        }

        /// <summary>
        /// Returns the content of a HTTP response message.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetContentAsString(this HttpWebResponse response)
        {
            string rdo;

            using (var responseStream = response.GetResponseStream())
            {
                Encoding encoding;

                if (string.IsNullOrEmpty(response.CharacterSet) || response.CharacterSet.Trim() == string.Empty)
                {
                    encoding = Encoding.Default;
                }
                else
                {
                    encoding = Encoding.GetEncoding(response.CharacterSet);
                }
                using (var reader = new System.IO.StreamReader(responseStream, encoding))
                {
                    rdo = reader.ReadToEnd();
                }
            }
            return rdo;
        }

        /// <summary>
        /// Gets a value that indicates whether the HTTP response was successful.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>
        /// A value that indicates whether the HTTP response was successful. 
        /// true if <see cref="HttpStatusCode"/> is in the Successful range (200-299); otherwise, false.
        /// </returns>
        public static bool IsSuccessStatusCode(this HttpWebResponse response)
        {
            var statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode < 300;
        }

        internal static bool IsContentType(this HttpWebResponse response, string mediaType)
        {
            if (response.ContentLength != 0)
            {
                return response.ContentType?
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Contains(mediaType, StringComparer.OrdinalIgnoreCase)
                    ?? false;
            }
            else
            {
                return false;
            }
        }

    }
}
#endif
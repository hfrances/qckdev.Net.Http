#if NO_SYNC || NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace qckdev.Net.Http
{
    public static class HttpWebResponseExtensions
    {
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
                string method = response.Method;
                TError errorContent;
                string reasonPhrase;

                if (response.IsContentType(Constants.MEDIATYPE_APPLICATIONJSON))
                {
                    var stringContent = response.GetContentAsString();

                    reasonPhrase = response.StatusDescription;
                    if (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty)
                    {
                        errorContent = default;
                    }
                    else if (options?.OnDeserializeError == null)
                    {
                        errorContent = JsonConvert.DeserializeObject<TError>(stringContent);
                    }
                    else
                    {
                        errorContent = options.OnDeserializeError(stringContent);
                    }
                }
                else if (response.IsContentType(Constants.MEDIATYPE_TEXTPLAIN))
                {
                    var stringContent = response.GetContentAsString();

                    reasonPhrase = (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty) ?
                        response.StatusDescription :
                        stringContent;
                    errorContent = default;
                }
                else
                {
                    reasonPhrase = response.StatusDescription;
                    errorContent = default;
                }
                throw new FetchFailedException<TError>(method, response.ResponseUri, response.StatusCode, reasonPhrase, errorContent);
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
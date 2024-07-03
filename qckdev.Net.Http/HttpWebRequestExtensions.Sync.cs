#if NO_SYNC || NO_WEB
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace qckdev.Net.Http
{
    public static partial class HttpWebRequestExtensions
    {

        /// <summary>
        /// Send an HTTP request.
        /// </summary>
        /// <typeparam name="TResult">The type of the response.</typeparam>
        /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Content"/>.</typeparam>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="options">Provides options for fetching process.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result.</returns>
        /// <exception cref="FetchFailedException{TError}">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
        /// The request returned a status code out of the range 200-299.
        /// </exception>
        public static TResult Fetch<TResult, TError>(this HttpWebRequest request, FetchOptions<TResult, TError> options = null)
        {

            try
            {
                HttpWebResponse response;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex) when (ex.Response != null)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                using (response)
                {
                    return response.DeserializeContent<TResult, TError>(options);
                }
            }
            catch (Exception ex)
            {
                throw CreateException<TError>(request, ex);
            }
        }

        /// <summary>
        /// Adds a JSON content to the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="content">The object to parse to JSON.</param>
        public static void SetContent(this HttpWebRequest request, object content)
        {
            var contentString = qckdev.Text.Json.JsonConvert.SerializeObject<object>(content);

            SetContent(request, contentString);
        }

        /// <summary>
        /// Adds a JSON content to the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="content">The JSON in string format.</param>
        public static void SetContent(this HttpWebRequest request, string content)
        {
            SetContent(request, content, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Adds a JSON content to the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpWebRequest"/> with the information to send.</param>
        /// <param name="content">The JSON in string format.</param>
        /// <param name="encoding">The encoding used for the <paramref name="content"/>.</param>
        public static void SetContent(this HttpWebRequest request, string content, System.Text.Encoding encoding)
        {
            request.ContentType = $"{Constants.MEDIATYPE_APPLICATIONJSON}; charset={encoding.EncodingName}";

            if (content != null)
            {
                var contentArray = encoding.GetBytes(content);

                request.ContentLength = contentArray.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(contentArray, 0, contentArray.Length);
                    stream.Close();
                }
            }
        }

        internal static string GetContentAsString(this HttpWebRequest request)
        {
            string rdo;

            if (request.ContentLength > 0)
            {
                using (var stream = request.GetRequestStream())
                {
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        rdo = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                rdo = null;
            }
            return rdo;
        }

    }
}
#endif
#if NO_WEB
#else
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace qckdev.Net.Http
{
    public static class WebClientExtensions
    {

        public static TResult Fetch<TResult, TError>(this WebClient client, string method, string requestUri, object content = null, FetchOptions<TResult, TError> options = null)
        {
            string contentString;

            if (content == null)
            {
                contentString = null;
            }
            else
            {
                contentString = JsonConvert.SerializeObject<object>(content);
            }
            return Fetch<TResult, TError>(client, method, requestUri, contentString, options);
        }

        public static TResult Fetch<TResult, TError>(this WebClient client, string method, string requestUri, string content, FetchOptions<TResult, TError> options = null)
        {
            var fullUri = (string.IsNullOrEmpty(client.BaseAddress) ? new Uri(requestUri) : new Uri(new Uri(client.BaseAddress), requestUri));

            try
            {
                string rdo;

                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    client.Headers.Remove(HttpRequestHeader.ContentType);
                    client.Headers.Remove("charset");
                    rdo = client.DownloadString(fullUri);
                }
                else
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add("charset", "'utf-8'");
                    rdo = client.UploadString(fullUri, method, content ?? string.Empty);
                }

                if (options?.OnDeserialize == null)
                {
                    return JsonConvert.DeserializeObject<TResult>(rdo);
                }
                else
                {
                    return options.OnDeserialize(rdo);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    TError errorContent;
                    string reasonPhrase;

                    if (httpResponse.IsContentType(Constants.MEDIATYPE_APPLICATIONJSON))
                    {
                        var stringContent = httpResponse.GetContentAsString();

                        reasonPhrase = httpResponse.StatusDescription;
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
                    else if (httpResponse.IsContentType(Constants.MEDIATYPE_TEXTPLAIN))
                    {
                        var stringContent = httpResponse.GetContentAsString();

                        reasonPhrase = (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty) ?
                            httpResponse.StatusDescription :
                            stringContent;
                        errorContent = default;
                    }
                    else
                    {
                        reasonPhrase = httpResponse.StatusDescription;
                        errorContent = default;
                    }
                    throw new FetchFailedException<TError>(method, httpResponse.ResponseUri, httpResponse.StatusCode, reasonPhrase, errorContent, ex);
                }
                else
                {
                    throw new FetchFailedException<TError>(method, ex.Response.ResponseUri, null, ex.Message, default, ex);
                }
            }
        }

    }
}
#endif
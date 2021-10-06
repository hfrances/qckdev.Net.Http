using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{


    static class HttpResponseMessageExtensions
    {


        public static async Task DeserializeContentAsync(this HttpResponseMessage response)
        {
            await DeserializeContentAsync<ExpandoObject, ExpandoObject>(response);
        }

        public static async Task<TResult> DeserializeContentAsync<TResult>(this HttpResponseMessage response)
        {
            return await DeserializeContentAsync<TResult, ExpandoObject>(response);
        }

        public static async Task<TResult> DeserializeContentAsync<TResult, TError>(this HttpResponseMessage response)
        {
            var contentType = response.GetContentType();

            if (response.IsSuccessStatusCode)
            {
                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return default;
                }
            }
            else
            {
                TError errorContent;
                string reasonPhrase;

                if (contentType.Equals(Constants.MEDIATYPE_APPLICATIONJSON, StringComparison.OrdinalIgnoreCase))
                {
                    reasonPhrase = response.ReasonPhrase;
                    errorContent = JsonConvert.DeserializeObject<TError>(await response.Content.ReadAsStringAsync());
                }
                else if (contentType.Equals(Constants.MEDIATYPE_TEXTPLAIN, StringComparison.OrdinalIgnoreCase))
                {
                    reasonPhrase = await response.Content.ReadAsStringAsync();
                    errorContent = default;
                }
                else
                {
                    reasonPhrase = response.ReasonPhrase;
                    errorContent = default;
                }
                throw new FetchFailedException<TError>(response.RequestMessage.Method, response.RequestMessage.RequestUri, response.StatusCode, reasonPhrase, errorContent);
            }
        }

        static string GetContentType(this HttpResponseMessage response)
        {
            if (response.Content?.Headers.ContentLength > 0)
            {
                return response.Content.Headers.ContentType.MediaType;
            }
            else
            {
                return null;
            }
        }

    }
}

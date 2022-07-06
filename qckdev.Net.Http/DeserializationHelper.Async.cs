﻿#if NO_ASYNC
#else
using qckdev.Text.Json;
using System;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public async static Task<ErrorHandleResponse<TError>> HandleErrorAsync<TError>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate, Func<Task<string>> getStatusDescriptionPredicate,
            Func<string, Task<TError>> deserializeErrorPredicate
        )
        {
            TError errorContent;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATIONJSON))
            {
                var stringContent = await getStringContentPredicate();

                reasonPhrase = await getStatusDescriptionPredicate();
                errorContent = await GetErrorContentAsync(stringContent, deserializeErrorPredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXTPLAIN))
            {
                var stringContent = await getStringContentPredicate();

                reasonPhrase = (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty) ?
                    await getStatusDescriptionPredicate() :
                    stringContent;
                errorContent = default;
            }
            else
            {
                reasonPhrase = await getStatusDescriptionPredicate();
                errorContent = default;
            }
            return new ErrorHandleResponse<TError>()
            {
                ErrorContent = errorContent,
                ReasonPhrase = reasonPhrase
            };
        }


        static async Task<TError> GetErrorContentAsync<TError>(string stringContent, Func<string, Task<TError>> deserializeErrorPredicate)
        {
            TError errorContent;

            if (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty)
            {
                errorContent = default;
            }
            else if (deserializeErrorPredicate == null)
            {
                errorContent = JsonConvert.DeserializeObject<TError>(stringContent);
            }
            else
            {
                errorContent = await deserializeErrorPredicate(stringContent);
            }
            return errorContent;
        }

    }
}
#endif
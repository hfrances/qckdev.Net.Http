#if NO_ASYNC
#else
using qckdev.Text.Json;
using System;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public async static Task<TResult> HandleResponseAsync<TResult>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate,
            Func<string, Task<TResult>> deserializePredicate
        )
        {
            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON))
            {
                var stringContent = await getStringContentPredicate();

                return await GetContentAsync(stringContent, deserializePredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_HTML)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_CSV))
            {
                return (TResult)Convert.ChangeType(await getStringContentPredicate(), typeof(TResult));
            }
            else
            {
                return default;
            }
        }

        public async static Task<ErrorHandleResponse<TError>> HandleErrorAsync<TError>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate, Func<Task<string>> getStatusDescriptionPredicate,
            Func<string, Task<TError>> deserializeErrorPredicate
        )
        {
            string stringContent;
            TError errorContent;
            string reasonPhrase;

            try
            {
                stringContent = await getStringContentPredicate();
                if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATIONJSON) || isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON))
                {
                    reasonPhrase = await getStatusDescriptionPredicate();
                    errorContent = await GetContentAsync(stringContent, deserializeErrorPredicate);
                }
                else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN))
                {
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
                    ReasonPhrase = reasonPhrase,
                    ContentString = stringContent,
                    Content = errorContent
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        static async Task<TResult> GetContentAsync<TResult>(string stringContent, Func<string, Task<TResult>> deserializePredicate)
        {
            TResult result;

            if (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty)
            {
                result = default;
            }
            else if (deserializePredicate == null)
            {
                result = JsonConvert.DeserializeObject<TResult>(stringContent);
            }
            else
            {
                result = await deserializePredicate(stringContent);
            }
            return result;
        }

    }
}
#endif
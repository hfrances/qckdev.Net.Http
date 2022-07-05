#if NO_SYNC
#else
using qckdev.Text.Json;
using System;

namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public static ErrorHandleResponse<TError> HandleError<TError>(
            Func<string, bool> isContentTypePredicate, Func<string> getStringContentPredicate, Func<string> getStatusDescriptionPredicate,
            Func<string, TError> deserializeErrorPredicate
        )
        {
            TError errorContent;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATIONJSON))
            {
                var stringContent = getStringContentPredicate();

                reasonPhrase = getStatusDescriptionPredicate();
                errorContent = GetErrorContent(stringContent, deserializeErrorPredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXTPLAIN))
            {
                var stringContent = getStringContentPredicate();

                reasonPhrase = (string.IsNullOrEmpty(stringContent) || stringContent.Trim() == string.Empty) ?
                    getStatusDescriptionPredicate() :
                    stringContent;
                errorContent = default;
            }
            else
            {
                reasonPhrase = getStatusDescriptionPredicate();
                errorContent = default;
            }
            return new ErrorHandleResponse<TError>()
            {
                ErrorContent = errorContent,
                ReasonPhrase = reasonPhrase
            };
        }

        static TError GetErrorContent<TError>(string stringContent, Func<string, TError> deserializeErrorPredicate)
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
                errorContent = deserializeErrorPredicate(stringContent);
            }
            return errorContent;
        }

    }
}
#endif
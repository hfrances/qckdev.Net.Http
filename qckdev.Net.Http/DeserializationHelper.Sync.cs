#if NO_SYNC
#else
using qckdev.Text.Json;
using System;

namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public static TResult HandleResponse<TResult>(
            Func<string, bool> isContentTypePredicate, Func<string> getStringContentPredicate,
            Func<string, TResult> deserializePredicate
        )
        {
            
            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON))
            {
                var stringContent = getStringContentPredicate();

                return GetContent(stringContent, deserializePredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_HTML)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_CSV))
            {
                return (TResult)Convert.ChangeType(getStringContentPredicate(), typeof(TResult));
            }
            else
            {
                return default;
            }
        }

        public static ErrorHandleResponse<TError> HandleError<TError>(
            Func<string, bool> isContentTypePredicate, Func<string> getStringContentPredicate, Func<string> getStatusDescriptionPredicate,
            Func<string, TError> deserializeErrorPredicate
        )
        {
            TError errorContent;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON) || isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_PROBLEM_JSON))
            {
                var stringContent = getStringContentPredicate();

                reasonPhrase = getStatusDescriptionPredicate();
                errorContent = GetContent(stringContent, deserializeErrorPredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_HTML)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_CSV))
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

        static TResult GetContent<TResult>(string stringContent, Func<string, TResult> deserializePredicate)
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
                result = deserializePredicate(stringContent);
            }
            return result;
        }

    }
}
#endif
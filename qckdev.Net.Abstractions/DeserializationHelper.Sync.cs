#if NO_SYNC
#else
using qckdev.Text.Json;
using System;

namespace qckdev.Net
{
    public static partial class DeserializationHelper
    {

        /// <summary>
        /// Handles the response content based on its content type, and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize the response to.</typeparam>
        /// <param name="isContentTypePredicate">A predicate to check the content type.</param>
        /// <param name="getStringContentPredicate">A function to get the string content.</param>
        /// <param name="deserializePredicate">A function to deserialize the content to the specified type.</param>
        /// <returns>The deserialized content.</returns>
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

        /// <summary>
        /// Handles the error response content based on its content type, and creates an error response object.
        /// </summary>
        /// <typeparam name="TError">The type to deserialize the error content to.</typeparam>
        /// <param name="isContentTypePredicate">A predicate to check the content type.</param>
        /// <param name="getStringContentPredicate">A function to get the string content.</param>
        /// <param name="getStatusDescriptionPredicate">A function to get the status description.</param>
        /// <param name="deserializeErrorPredicate">A function to deserialize the error content.</param>
        /// <returns>The error response object.</returns>
        public static ErrorHandleResponse<TError> HandleError<TError>(
            Func<string, bool> isContentTypePredicate, Func<string> getStringContentPredicate, Func<string> getStatusDescriptionPredicate,
            Func<string, TError> deserializeErrorPredicate
        )
        {
            string contentString;
            TError content;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON) || isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_PROBLEM_JSON))
            {
                contentString = getStringContentPredicate();

                reasonPhrase = getStatusDescriptionPredicate();
                content = GetContent(contentString, deserializeErrorPredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_HTML)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_CSV))
            {
                contentString = getStringContentPredicate();

                reasonPhrase = (string.IsNullOrEmpty(contentString) || contentString.Trim() == string.Empty) ?
                    getStatusDescriptionPredicate() :
                    contentString;
                content = default;
            }
            else
            {
                contentString = getStringContentPredicate();
                reasonPhrase = getStatusDescriptionPredicate();
                content = default;
            }
            return new ErrorHandleResponse<TError>()
            {
                Content = content,
                ContentString = contentString,
                ReasonPhrase = reasonPhrase
            };
        }

        /// <summary>
        /// Gets the content from a string and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize the content to.</typeparam>
        /// <param name="stringContent">The string content to deserialize.</param>
        /// <param name="deserializePredicate">A function to deserialize the content to the specified type.</param>
        /// <returns>The deserialized content.</returns>
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



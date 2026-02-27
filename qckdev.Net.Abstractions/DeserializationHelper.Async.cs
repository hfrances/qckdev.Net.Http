#if NO_ASYNC
#else
using qckdev.Text.Json;
using System;
using System.Threading.Tasks;

namespace qckdev.Net.Http
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
        /// <returns>A task representing the deserialized content.</returns>
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

        /// <summary>
        /// Handles the error response content based on its content type, and creates an error response object.
        /// </summary>
        /// <typeparam name="TError">The type to deserialize the error content to.</typeparam>
        /// <param name="isContentTypePredicate">A predicate to check the content type.</param>
        /// <param name="getStringContentPredicate">A function to get the string content.</param>
        /// <param name="getStatusDescriptionPredicate">A function to get the status description.</param>
        /// <param name="deserializeErrorPredicate">A function to deserialize the error content.</param>
        /// <returns>A task representing the error response object.</returns>
        public async static Task<ErrorHandleResponse<TError>> HandleErrorAsync<TError>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate, Func<Task<string>> getStatusDescriptionPredicate,
            Func<string, Task<TError>> deserializeErrorPredicate
        )
        {
            TError errorContent;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON) || isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_PROBLEM_JSON))
            {
                var stringContent = await getStringContentPredicate();

                reasonPhrase = await getStatusDescriptionPredicate();
                errorContent = await GetContentAsync(stringContent, deserializeErrorPredicate);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN))
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

        /// <summary>
        /// Gets the content from a string and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize the content to.</typeparam>
        /// <param name="stringContent">The string content to deserialize.</param>
        /// <param name="deserializePredicate">A function to deserialize the content to the specified type.</param>
        /// <returns>A task representing the deserialized content.</returns>
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

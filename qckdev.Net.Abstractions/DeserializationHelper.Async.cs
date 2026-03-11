#if NO_ASYNC
#else
using qckdev.Text.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace qckdev.Net
{
    /// <summary>
    /// Provides helper methods to deserialize response and error content in asynchronous flows.
    /// </summary>
    public static partial class DeserializationHelper
    {
        
        /// <summary>
        /// Handles the response content based on its content type, and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize the response to.</typeparam>
        /// <param name="isContentTypePredicate">A predicate to check the content type.</param>
        /// <param name="getStringContentPredicate">A function to get the string content.</param>
        /// <param name="deserializePredicate">A function to deserialize the content to the specified type.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the deserialized content.</returns>
        public async static Task<TResult> HandleResponseAsync<TResult>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate,
            Func<string, Task<TResult>> deserializePredicate,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON))
            {
                var stringContent = await getStringContentPredicate();
                cancellationToken.ThrowIfCancellationRequested();

                return await GetContentAsync(stringContent, deserializePredicate, cancellationToken);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_HTML)
                || isContentTypePredicate(Constants.MEDIATYPE_TEXT_CSV))
            {
                var textContent = await getStringContentPredicate();
                cancellationToken.ThrowIfCancellationRequested();
                return (TResult)Convert.ChangeType(textContent, typeof(TResult));
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
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the error response object.</returns>
        public async static Task<ErrorHandleResponse<TError>> HandleErrorAsync<TError>(
            Func<string, bool> isContentTypePredicate, Func<Task<string>> getStringContentPredicate, Func<Task<string>> getStatusDescriptionPredicate,
            Func<string, Task<TError>> deserializeErrorPredicate,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            string contentString;
            TError content;
            string reasonPhrase;

            if (isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_JSON) || isContentTypePredicate(Constants.MEDIATYPE_APPLICATION_PROBLEM_JSON))
            {
                contentString = await getStringContentPredicate();
                cancellationToken.ThrowIfCancellationRequested();

                reasonPhrase = await getStatusDescriptionPredicate();
                cancellationToken.ThrowIfCancellationRequested();
                content = await GetContentAsync(contentString, deserializeErrorPredicate, cancellationToken);
            }
            else if (isContentTypePredicate(Constants.MEDIATYPE_TEXT_PLAIN))
            {
                contentString = await getStringContentPredicate();
                cancellationToken.ThrowIfCancellationRequested();

                reasonPhrase = (string.IsNullOrEmpty(contentString) || contentString.Trim() == string.Empty) ?
                    await getStatusDescriptionPredicate() :
                    contentString;
                content = default;
            }
            else
            {
                contentString = await getStringContentPredicate();
                cancellationToken.ThrowIfCancellationRequested();
                reasonPhrase = await getStatusDescriptionPredicate();
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
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the deserialized content.</returns>
        static async Task<TResult> GetContentAsync<TResult>(string stringContent, Func<string, Task<TResult>> deserializePredicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                cancellationToken.ThrowIfCancellationRequested();
            }
            return result;
        }

    }
}
#endif


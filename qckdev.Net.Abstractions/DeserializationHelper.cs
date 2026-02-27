namespace qckdev.Net.Http
{
    public static partial class DeserializationHelper
    {

        /// <summary>
        /// Represents a response for error handling during HTTP operations.
        /// </summary>
        /// <typeparam name="TError">The type of the error content.</typeparam>
        public class ErrorHandleResponse<TError>
        {
            /// <summary>
            /// Gets or sets the error content.
            /// </summary>
            public TError ErrorContent { get; set; }

            /// <summary>
            /// Gets or sets the reason phrase, which typically is sent by servers together with the status code.
            /// </summary>
            public string ReasonPhrase { get; set; }
        }
        
    }
}

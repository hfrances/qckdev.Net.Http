namespace qckdev.Net
{
    /// <summary>
    /// Provides helper methods and models to deserialize response and error content.
    /// </summary>
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
            public TError Content { get; set; }

            /// <summary>
            /// Gets or sets the raw response content for error handling.
            /// </summary>
            public string ContentString { get; set; }

            /// <summary>
            /// Gets or sets the reason phrase, which typically is sent by servers together with the status code.
            /// </summary>
            public string ReasonPhrase { get; set; }
        }
        
    }
}



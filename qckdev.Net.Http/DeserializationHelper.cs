
namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public class ErrorHandleResponse<TError>
        {
            public TError ErrorContent { get; set; }
            public string ReasonPhrase { get; set; }
        }

    }
}

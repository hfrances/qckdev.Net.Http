
namespace qckdev.Net.Http
{
    static partial class DeserializationHelper
    {

        public class ErrorHandleResponse<TError>
        {
            public string ReasonPhrase { get; set; }
            public TError Content { get; set; }
            public string ContentString { get; set; }
            
        }

    }
}

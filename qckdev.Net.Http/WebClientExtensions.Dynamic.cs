#if NO_WEB
#else
using System.Net;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Provides extension methods for <see cref="WebClient"/>.
    /// </summary>
    public static partial class WebClientExtensions
    {

#if NO_DYNAMIC
        public static object Fetch(this WebClient client, string method, string requestUri, object content = null, FetchOptions<object> options = null)
        {
            return Fetch<object>(client, method, requestUri, content, options);
        }
#else
        public static dynamic Fetch(this WebClient client, string method, string requestUri, object content = null, FetchOptions<System.Dynamic.ExpandoObject> options = null)
        {
            return Fetch<System.Dynamic.ExpandoObject, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
        }
#endif

        public static TResult Fetch<TResult>(this WebClient client, string method, string requestUri, object content = null, FetchOptions<TResult> options = null)
        {
#if NO_DYNAMIC
            return Fetch<TResult, object>(client, method, requestUri, content, options);
#else
            return Fetch<TResult, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
#endif
        }

        public static TResult Fetch<TResult>(this WebClient client, string method, string requestUri, string content, FetchOptions<TResult> options = null)
        {
#if NO_DYNAMIC
            return Fetch<TResult, object>(client, method, requestUri, content, options);
#else
            return Fetch<TResult, System.Dynamic.ExpandoObject>(client, method, requestUri, content, options);
#endif            
        }

    }
}
#endif
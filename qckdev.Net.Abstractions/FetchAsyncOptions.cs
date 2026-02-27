#if NO_ASYNC
#else
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Specifies the settings for fetching process.
    /// </summary>
    /// <typeparam name="TResult">The type of the response.</typeparam>
    /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Error"/>.</typeparam>
    public class FetchAsyncOptions<TResult, TError>
    {
        
        /// <summary>
        /// Gets or sets the function to deserialize the response content to <typeparamref name="TResult"/>.
        /// </summary>
        public Func<string, Task<TResult>> OnDeserializeAsync { get; set; }

        /// <summary>
        /// Gets or sets the function to deserialize the error response content to <typeparamref name="TError"/>.
        /// </summary>
        public Func<string, Task<TError>> OnDeserializeErrorAsync { get; set; }

    }

    /// <summary>
    /// Specifies the settings for fetching process.
    /// </summary>
    /// <typeparam name="TResult">The type of the response.</typeparam>
    public class FetchAsyncOptions<TResult> : FetchAsyncOptions<TResult, ExpandoObject>
    { }

}
#endif
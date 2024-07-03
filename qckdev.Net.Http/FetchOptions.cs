using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http
{

    /// <summary>
    /// Specifies the settings for fetching process.
    /// </summary>
    /// <typeparam name="TResult">The type of the response.</typeparam>
    /// <typeparam name="TError">The type of the <see cref="FetchFailedException{TError}.Content"/>.</typeparam>
    public class FetchOptions<TResult, TError>
    {

        public Func<string, TResult> OnDeserialize { get; set; }
        public Func<string, TError> OnDeserializeError { get; set; }

    }

    /// <summary>
    /// Specifies the settings for fetching process.
    /// </summary>
    /// <typeparam name="TResult">The type of the response.</typeparam>
#if NO_DYNAMIC
    public class FetchOptions<TResult> : FetchOptions<TResult, object>
#else
    public class FetchOptions<TResult> : FetchOptions<TResult, System.Dynamic.ExpandoObject>
#endif
    { }

}

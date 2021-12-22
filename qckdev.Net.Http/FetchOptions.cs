using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace qckdev.Net.Http
{
    public class FetchOptions<TResult, TError>
    {

        public Func<string, TResult> OnDeserialize { get; set; }
        public Func<string, TError> OnDeserializeError { get; set; }

    }

    public class FetchOptions<TResult> : FetchOptions<TResult, ExpandoObject>
    { }

}

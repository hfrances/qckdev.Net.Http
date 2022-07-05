using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http.Test.Common.TestObjects
{
    public sealed class GoResponse<TData>
    {

        public object Meta { get; set; }
        public TData Data { get; set; }

    }
}

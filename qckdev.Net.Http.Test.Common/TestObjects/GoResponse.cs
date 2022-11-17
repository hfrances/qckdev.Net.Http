using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http.Test.Common.TestObjects
{
    public class GoResponse
    {

        public object Meta { get; set; }
        public object Data { get; set; }

    }

    public sealed class GoResponse<TData> : GoResponse
    {

        public new TData Data
        {
            get => (TData)base.Data;
            set => base.Data = value;
        }

    }
}

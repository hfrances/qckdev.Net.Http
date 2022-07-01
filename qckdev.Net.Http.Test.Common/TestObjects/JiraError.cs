using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http.Test.Common.TestObjects
{
    public sealed class JiraError
    {
        public IEnumerable<string> ErrorMessages { get; set; }
        public JiraErrorDetails Errors { get; set; }
    }
}

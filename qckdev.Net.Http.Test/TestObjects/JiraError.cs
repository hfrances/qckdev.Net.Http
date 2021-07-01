using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http.Test.TestObjects
{
    sealed class JiraError
    {
        public IEnumerable<string> ErrorMessages { get; set; }
        public JiraErrorDetails Errors { get; set; }
    }
}

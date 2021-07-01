using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Net.Http.Test.TestObjects
{
    sealed class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public Species Species { get; set; }

    }
}

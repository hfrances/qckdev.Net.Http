using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qckdev.Net.Http
{
    public sealed class DeserializationException : Exception
    {

        public string ContentString { get; }

        public DeserializationException(string message, string stringContent)
            : this(message, stringContent, innerException: null)
        {

        }

        public DeserializationException(string message, string stringContent, Exception innerException)
            : base(message, innerException)
        {
            this.ContentString = stringContent;
        }

    }
}

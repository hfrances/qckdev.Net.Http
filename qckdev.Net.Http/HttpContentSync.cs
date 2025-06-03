#if NO_HTTP
#else
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using qckdev.Net.Http.Headers;

namespace qckdev.Net.Http
{
    /// <summary>
    /// Base class for HTTP content in synchronous operations.
    /// </summary>
    abstract class HttpContentSync : IDisposable
    {
        
        /// <summary>
        /// Gets the default encoding for strings (UTF-8).
        /// </summary>
        internal static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

#if NETSTANDARD1_2
        /// <summary>
        /// Gets the headers for the HTTP content.
        /// </summary>
        public HttpContentHeaderSync Headers { get; }
#else
        /// <summary>
        /// Gets the headers for the HTTP content.
        /// </summary>
        public HttpContentHeaders Headers { get; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContentSync"/> class.
        /// </summary>
        protected HttpContentSync()
        {
#if NETSTANDARD1_2
            this.Headers = new HttpContentHeaderSync();
#else
            this.Headers = CreateHeader();
#endif
        }

        /// <summary>
        /// Returns the HTTP content as a stream.
        /// </summary>
        /// <returns>A stream that represents the content.</returns>
        public Stream ReadAsStream()
        {
            return CreateContentReadStream();
        }

        /// <summary>
        /// When overridden in a derived class, creates a stream for reading the content.
        /// </summary>
        /// <returns>A stream that represents the content.</returns>
        protected abstract Stream CreateContentReadStream();

        /// <summary>
        /// Releases the unmanaged resources and disposes of the managed resources used by the <see cref="HttpContentSync"/>.
        /// </summary>
        public void Dispose()
        {
            // TODO.
        }



#if NETSTANDARD1_2
#else
        /// <summary>
        /// Creates an HTTP content headers instance.
        /// </summary>
        /// <returns>A new instance of HttpContentHeaders.</returns>
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
        private static HttpContentHeaders CreateHeader()
        {
            return (HttpContentHeaders)Activator.CreateInstance(
                typeof(HttpContentHeaders),
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { (HttpContent)null },
                CultureInfo.InvariantCulture);
        }
#endif
    }
}
#endif
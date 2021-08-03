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
    abstract class HttpContentSync : IDisposable
    {

        internal static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

#if STANDARD12
        public HttpContentHeaderSync Headers { get; }
#else
        public HttpContentHeaders Headers { get; }
#endif

        protected HttpContentSync()
        {
#if STANDARD12
            this.Headers = new HttpContentHeaderSync();
#else
            this.Headers = CreateHeader();
#endif
        }

        public Stream ReadAsStream()
        {
            return CreateContentReadStream();
        }

        protected abstract Stream CreateContentReadStream();

        public void Dispose()
        {
            // TODO.
        }



#if STANDARD12
#else
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
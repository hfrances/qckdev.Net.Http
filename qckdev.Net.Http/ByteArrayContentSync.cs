#if NO_HTTP
#else
using qckdev.Net;
using System;
using System.IO;

namespace qckdev.Net.Http
{
    /// <summary>
    /// Provides HTTP content based on a byte array.
    /// </summary>
    class ByteArrayContentSync : HttpContentSync
    {
        /// <summary>
        /// Gets the content as a byte array.
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// Gets the offset within the byte array where the content starts.
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Gets the number of bytes in the content.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayContentSync"/> class with a byte array.
        /// </summary>
        /// <param name="content">The content as a byte array.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is null.</exception>
        public ByteArrayContentSync(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.Content = content;
            this.Count = content.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayContentSync"/> class with a byte array, an offset, and a count.
        /// </summary>
        /// <param name="content">The content as a byte array.</param>
        /// <param name="offset">The offset in the byte array from which to start.</param>
        /// <param name="count">The number of bytes to include.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="offset"/> is less than zero or greater than the length of <paramref name="content"/>.
        /// -or-
        /// The <paramref name="count"/> is less than zero or greater than the length of content minus offset.
        /// </exception>
        public ByteArrayContentSync(byte[] content, int offset, int count)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if ((offset < 0) || (offset > content.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((count < 0) || (count > (content.Length - offset)))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            this.Content = content;
            this.Offset = offset;
            this.Count = count;
        }

        /// <summary>
        /// Creates a memory stream from the byte array.
        /// </summary>
        /// <returns>A memory stream containing the byte array data.</returns>
        private MemoryStream CreateMemoryStreamForByteArray()
            => new MemoryStream(this.Content, this.Offset, this.Count, writable: false);

        /// <summary>
        /// Creates a stream for reading the content.
        /// </summary>
        /// <returns>A stream containing the content.</returns>
        protected override Stream CreateContentReadStream()
        {
            return CreateMemoryStreamForByteArray();
        }
        
    }
}
#endif




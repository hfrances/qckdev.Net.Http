using System;
using System.IO;

namespace qckdev.Net.Http
{
    class ByteArrayContentSync : HttpContentSync
    {

        byte[] Content { get; }
        int Offset { get; }
        int Count { get; }

        public ByteArrayContentSync(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            this.Content = content;
            this.Count = content.Length;
        }

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


        private MemoryStream CreateMemoryStreamForByteArray() 
            => new MemoryStream(this.Content, this.Offset, this.Count, writable: false);

        protected override Stream CreateContentReadStream()
        {
            return CreateMemoryStreamForByteArray();
        }

    }
}
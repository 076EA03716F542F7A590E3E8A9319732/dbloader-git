
using System.Runtime.Remoting.Messaging;

namespace DbLoader
{
    public class LCGStream : Stream
    {
        public LCGStream(Stream input, uint key1, uint key2, uint key3)
        {
            this.input = input;
            this.key1 = key1;
            this.key2 = key2;
            this.key3 = key3;
        }

        public override long Position
        {
            get => this.input.Position;
            set => throw new NotSupportedException();
        }

        public override long Length => this.input.Length;

        public override bool CanWrite => false;

        public override bool CanSeek => false;

        public override bool CanRead => this.input.CanRead;

        public override void Flush() => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = this.input.Read(buffer, offset, count);
            for (int i = 0; i < offset + num; i++)
            {
                buffer[i] ^= (byte)(this.key1 >> 24 ^ this.key2 >> 24 ^ this.key3 >> 24);
                this.key1 = this.key1 * 214013U + 2531011U;
                this.key2 = this.key2 * 214013U + 2531011U;
                this.key3 = this.key3 * 214013U + 2531011U;
            }
            return num;
        }

        private Stream input;

        private uint key1;

        private uint key2;

        private uint key3;
    }
}
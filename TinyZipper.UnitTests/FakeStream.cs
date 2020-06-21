using System.IO;

namespace TinyZipper.UnitTests
{
    public class FakeStream : MemoryStream
    {
        private MemoryStream _internalStream = new MemoryStream();

        public override void Flush()
        {
            _internalStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _internalStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _internalStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _internalStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _internalStream.Write(buffer, offset, count);
        }

        public override bool CanRead => _internalStream.CanRead;

        public override bool CanSeek => _internalStream.CanSeek;

        public override bool CanWrite => _internalStream.CanWrite;

        public override long Length => _internalStream.Length;

        public override long Position
        {
            get => _internalStream.Position;
            set => _internalStream.Position = value;
        }

        public override byte[] ToArray()
        {
            return _internalStream.ToArray();
        }

        public void ManualClose()
        {
            _internalStream.Close();
            _internalStream.Dispose();
        }
    }
}
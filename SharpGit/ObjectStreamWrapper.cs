using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NGit;

namespace SharpGit
{
    internal sealed class ObjectStreamWrapper : Stream
    {
        private ObjectStream _stream;
        private bool _disposed;

        public ObjectStreamWrapper(ObjectStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _stream = stream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return _stream.GetSize(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ObjectStreamWrapper;

            return
                other != null &&
                _stream.Equals(other._stream);
        }

        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }

        public override int ReadByte()
        {
            return _stream.Read();
        }

        public override string ToString()
        {
            return _stream.ToString();
        }

        public void CopyTo(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] buffer = new byte[4096];
            int read;

            while ((read = Read(buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, read);
            }
        }
    }
}

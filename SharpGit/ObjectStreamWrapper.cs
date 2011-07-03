// SharpGit\ObjectStreamWrapper.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
            // ObjectStream returns -1 at the end of the stream. Convert to 0.

            return Math.Max(_stream.Read(buffer, offset, count), 0);
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
    }
}

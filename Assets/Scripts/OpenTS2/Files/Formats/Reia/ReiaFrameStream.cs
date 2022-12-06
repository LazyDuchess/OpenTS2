using OpenTS2.Files.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFrameStream
    {
        public ReiaFrame Current => _enumerator.Current;
        protected IEnumerator<ReiaFrame> _enumerator;
        private readonly IoBuffer _io;
        private readonly int _frameStartPosition;
        private readonly int _width;
        private readonly int _height;
        private readonly Stream _stream;

        public ReiaFrameStream(IEnumerator<ReiaFrame> enumerator, IoBuffer io, int frameStartPosition, int width, int height, Stream stream)
        {
            _enumerator = enumerator;
            _io = io;
            _frameStartPosition = frameStartPosition;
            _width = width;
            _height = height;
            _stream = stream;
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        void FreeCurrentFrame()
        {
            _enumerator.Current?.Dispose();
        }

        public virtual void Reset()
        {
            FreeCurrentFrame();
            _enumerator?.Current?.Dispose();
            _enumerator.Dispose();
            _io.Seek(System.IO.SeekOrigin.Begin, _frameStartPosition);
            _enumerator = ReiaFrame.ReadFrameEnumerable(_io, _width, _height).GetEnumerator();
        }

        public virtual void Dispose()
        {
            FreeCurrentFrame();
            _enumerator?.Current?.Dispose();
            _enumerator.Dispose();
            _io.Dispose();
            _stream.Dispose();
        }
    }
}

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
    public class ReiaFrameMemoryStream : ReiaFrameStream
    {
        public ReiaFrameMemoryStream(IEnumerator<ReiaFrame> enumerator, IoBuffer io, int frameStartPosition, int width, int height, Stream stream) : base(enumerator, io, frameStartPosition, width, height, stream)
        {

        }

        public override void Reset()
        {
            _enumerator.Reset();
        }

        public override void Dispose()
        {
            Reset();
            var item = _enumerator.MoveNext();
            while (item)
            {
                _enumerator.Current?.Dispose();
                item = _enumerator.MoveNext();
            }
        }
    }
}

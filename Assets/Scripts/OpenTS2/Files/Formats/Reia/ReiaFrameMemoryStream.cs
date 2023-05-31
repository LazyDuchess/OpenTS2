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
        private List<ReiaFrame> _frameList;
        public ReiaFrameMemoryStream(IEnumerator<ReiaFrame> enumerator, List<ReiaFrame> frameList, IoBuffer io, int frameStartPosition, int width, int height, Stream stream) : base(enumerator, io, frameStartPosition, width, height, stream)
        {
            _frameList = frameList;
        }

        public override void Reset()
        {
            _enumerator.Reset();
        }

        public override void Dispose()
        {
            foreach (var frame in _frameList)
                frame.Dispose();
        }
    }
}

using OpenTS2.Engine;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFrame : IDisposable
    {
        public virtual void Dispose()
        {
            
        }

        public static IEnumerable<ReiaFrame> ReadFrameEnumerable(IoBuffer io, int width, int height)
        {
            return ReiaImplementation.Get().ReadFrameEnumerable(io, width, height);
        }

        public static List<ReiaFrame> ReadFrames(IoBuffer io, int width, int height)
        {
            return ReiaImplementation.Get().ReadFrames(io, width, height);
        }
    }
}

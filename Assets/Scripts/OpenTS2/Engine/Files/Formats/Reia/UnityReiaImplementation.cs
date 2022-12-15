using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Files.Formats.Reia;
using OpenTS2.Files.Utils;

namespace OpenTS2.Engine.Files.Formats.Reia
{
    /// <summary>
    /// Unity implementation of Reia video files.
    /// </summary>
    class UnityReiaImplementation : ReiaImplementation
    {
        public override IEnumerable<ReiaFrame> ReadFrameEnumerable(IoBuffer io, int width, int height)
        {
            return UnityReiaFrame.UnityReadFrameEnumerable(io, width, height);
        }

        public override List<ReiaFrame> ReadFrames(IoBuffer io, int width, int height)
        {
            return UnityReiaFrame.UnityReadFrames(io, width, height);
        }
    }
}

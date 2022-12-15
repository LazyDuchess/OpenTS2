using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.Reia
{
    public abstract class ReiaImplementation
    {
        /// <summary>
        /// Get current ReiaImplementation Singleton.
        /// </summary>
        /// <returns>ReiaImplementation Singleton.</returns>
        public static ReiaImplementation Get()
        {
            return s_instance;
        }

        /// <summary>
        /// Static ReiaImplementation Singleton instance.
        /// </summary>
        static ReiaImplementation s_instance;

        /// <summary>
        /// Construct a new ReiaImplementation, make it the new Singleton.
        /// </summary>
        public ReiaImplementation()
        {
            s_instance = this;
        }

        public abstract IEnumerable<ReiaFrame> ReadFrameEnumerable(IoBuffer io, int width, int height);

        public abstract List<ReiaFrame> ReadFrames(IoBuffer io, int width, int height);
    }
}

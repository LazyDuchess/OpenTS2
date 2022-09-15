using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Common.Utils
{
    public static class StreamUtils
    {
        /// <summary>
        /// Get buffer from MemoryStream, using its current position as the size.
        /// </summary>
        /// <param name="stream">MemoryStream to get byte buffer from.</param>
        /// <returns>Byte buffer.</returns>
        public static byte[] GetBuffer(MemoryStream stream)
        {
            return GetBuffer(stream, (int)stream.Position);
        }
        /// <summary>
        /// Get buffer from MemoryStream with a set size.
        /// </summary>
        /// <param name="stream">MemoryStream to get byte buffer from.</param>
        /// <param name="length">Length of byte buffer.</param>
        /// <returns>Byte buffer.</returns>
        public static byte[] GetBuffer(MemoryStream stream, int length)
        {
            var streamBuffer = stream.GetBuffer();
            if (length > streamBuffer.Length)
                length = streamBuffer.Length;
            var buff = new byte[length];
            Array.Copy(streamBuffer, buff, length);
            return buff;
        }
    }
}

/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Text;
using InvertedTomato.IO;

namespace OpenTS2.Common.Utils
{
    /// <summary>
    /// Filesystem utilities.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Generates a CRC32 Hash for High Instance IDs out of a string.
        /// </summary>
        /// <param name="name">String to hash.</param>
        /// <returns>Hashed result.</returns>
        public static uint HighHash(string name)
        {
            name = name.Trim().ToLower();
            var crc = CrcAlgorithm.CreateCrc32Mpeg2();
            crc.Append(Encoding.ASCII.GetBytes(name));
            return (uint)crc.ToUInt64();
        }

        /// <summary>
        /// Generates a CRC24 Hash for Instance IDs out of a string.
        /// </summary>
        /// <param name="name">String to hash.</param>
        /// <returns>Hashed result.</returns>
        public static uint LowHash(string name)
        {
            name = name.Trim().ToLower();
            var crc = CrcAlgorithm.CreateCrc24();
            crc.Append(Encoding.ASCII.GetBytes(name));
            return (uint)((crc.ToUInt64() & 0x00ffffff) | 0xff000000);
        }

        /// <summary>
        /// Generates a CRC24 Hash for Group IDs out of a string.
        /// </summary>
        /// <param name="name">String to hash.</param>
        /// <returns>Hashed result.</returns>
        public static uint GroupHash(string name)
        {
            name = name.Trim().ToLower();
            var crc = CrcAlgorithm.CreateCrc24();
            crc.Append(Encoding.ASCII.GetBytes(name));
            return (uint)((crc.ToUInt64() & 0x00ffffff) | 0x7f000000);
        }
    }
}

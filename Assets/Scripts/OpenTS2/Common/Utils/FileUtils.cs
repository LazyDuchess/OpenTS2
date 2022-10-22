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
        /// Cleans up a path to use normal slashes and cut off the slash at the end if there is one.
        /// </summary>
        /// <param name="path">Path to clean up</param>
        /// <returns>Cleaned up path</returns>
        public static string CleanPath(string path)
        {
            //Trim whitespaces
            path = path.Trim();
            //Remove double slashes
            var finalPath = "";
            var lastwasSlash = false;
            for(var i=0;i<path.Length;i++)
            {
                var cha = path[i];
                if (cha == '\\' || cha == '/')
                {
                    if (!lastwasSlash)
                    {
                        finalPath += "/";
                        lastwasSlash = true;
                    }
                }
                else
                {
                    lastwasSlash = false;
                    finalPath += cha;
                }
            }
            //Remove slash if there is a redundant one at the end
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);
            path = path.Replace("\\", "/");
            return path;
        }
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

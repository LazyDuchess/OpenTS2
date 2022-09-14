/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTS2.Content.Interfaces;
using OpenTS2.Common.Utils;

namespace OpenTS2.Files
{
    /// <summary>
    /// Handles device filesystem interfacing and path parsing
    /// </summary>
    public class Filesystem
    {
        string _DataDirectory;
        string _UserDirectory;
        string _BinDirectory;

        public string UserDataDirectory
        {
            get { return "%UserDataDir%"; }
        }

        public string DataDirectory
        {
            get { return "%DataDirectory%"; }
        }

        public string BinDirectory
        {
            get { return "%BinDirectory%"; }
        }

        public Filesystem(IPathProvider pathProvider)
        {
            _DataDirectory = FileUtils.CleanPath(pathProvider.GetGameRootPath()) + "/TSData/";
            _UserDirectory = FileUtils.CleanPath(pathProvider.GetUserPath()) + "/";
            _BinDirectory = FileUtils.CleanPath(pathProvider.GetGameRootPath()) + "/TSBin/";
        }
        /// <summary>
        /// Checks if two paths, unparsed or parsed, are equal.
        /// </summary>
        /// <param name="path1">Path to check</param>
        /// <param name="path2">Path to check against</param>
        /// <returns>True if equal, false if not.</returns>
        public bool PathsEqual(string path1, string path2)
        {
            path1 = GetRealPath(path1).ToLowerInvariant();
            path2 = GetRealPath(path2).ToLowerInvariant();
            return (path1 == path2);
        }
        /// <summary>
        /// Returns short relative path. (Eg. Replaces the game's directory with the %DataDirectory% shorthand)
        /// </summary>
        /// <param name="path">Path to shorten</param>
        /// <returns>Fake short path</returns>
        public string GetShortPath(string path)
        {
            path = FileUtils.CleanPath(path) + "/";
            path = path.Replace(_DataDirectory, DataDirectory);
            path = path.Replace(_UserDirectory, UserDataDirectory);
            path = path.Replace(_BinDirectory, BinDirectory);
            path = FileUtils.CleanPath(path);
            return path;
        }

        /// <summary>
        /// Returns fully parsed path. (Eg. Replaces %DataDirectory% with actual data directory path)
        /// </summary>
        /// <param name="path">Path to parse</param>
        /// <returns>Real path</returns>
        public string GetRealPath(string path)
        {
            path = path.Replace(DataDirectory, _DataDirectory);
            path = path.Replace(UserDataDirectory, _UserDirectory);
            path = path.Replace(BinDirectory, _BinDirectory);
            path = FileUtils.CleanPath(path);
            return path;
        }
        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="path">Unparsed path</param>
        /// <returns>A FileStream</returns>
        public FileStream OpenWrite(string path)
        {
            return File.OpenWrite(GetRealPath(path));
        }

        /// <summary>
        /// Opens a file for reading.
        /// </summary>
        /// <param name="path">Unparsed path</param>
        /// <returns>A FileStream</returns>
        public FileStream OpenRead(string path)
        {
            return File.OpenRead(GetRealPath(path));
        }

        /// <summary>
        /// Writes a byte array into a file.
        /// </summary>
        /// <param name="path">Path to output file.</param>
        /// <param name="bytes">Byte array to write.</param>
        public void Write(string path, byte[] bytes)
        {
            Write(path, bytes, bytes.Length);
        }

        /// <summary>
        /// Writes a byte array into a file.
        /// </summary>
        /// <param name="path">Path to output file.</param>
        /// <param name="bytes">Byte array to write.</param>
        public void Write(string path, byte[] bytes, int size)
        {
            var realPath = GetRealPath(path);
            var dir = Path.GetDirectoryName(realPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var fStream = new FileStream(realPath, FileMode.Create, FileAccess.Write);
            fStream.Write(bytes, 0, size);
            fStream.Dispose();
            //File.WriteAllBytes(realPath, bytes);
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="path">Path to file to delete.</param>
        public void Delete(string path)
        {
            var realPath = GetRealPath(path);
            File.Delete(path);
        }
    }
}

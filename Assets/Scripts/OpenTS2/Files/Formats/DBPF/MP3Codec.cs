/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using OpenTS2.Files.Utils;
using OpenTS2.Content;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using System.Text;
using OpenTS2.Files.Formats.XA;
using OpenTS2.Files.Formats.UTK;

namespace OpenTS2.Files.Formats.DBPF
{

    /// <summary>
    /// MP3 or XA audio reading code.
    /// </summary>
    [Codec(TypeIDs.MP3)]
    public class MP3Codec : AbstractCodec
    {

        /// <summary>
        /// Constructs a new MP3 instance.
        /// </summary>
        public MP3Codec()
        {

        }

        /// <summary>
        /// Parses MP3 from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var magic = Encoding.UTF8.GetString(bytes, 0, 2);
            byte[] data = new byte[] { };
            switch(magic)
            {
                case "XA":
                    var xa = new XAFile(bytes);
                    data = xa.DecompressedData;
                    break;
                case "UT":
                    var utk = new UTKFile(bytes);
                    utk.UTKDecode();
                    data = utk.DecompressedWav;
                    break;
            }
            return new AudioAsset(data);
        }
    }
}
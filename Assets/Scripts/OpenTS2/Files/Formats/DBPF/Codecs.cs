/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codecs for DBPF archives.
    /// </summary>
    public static class Codecs
    {
        public static STRCodec STR = new STRCodec();
        public static IMGCodec IMG = new IMGCodec();
        public static DIRCodec DIR = new DIRCodec();
        public static OBJDCodec OBJD = new OBJDCodec();
        public static MP3Codec MP3 = new MP3Codec();

        static Dictionary<uint, AbstractCodec> codecsByTypeID = new Dictionary<uint, AbstractCodec>()
        {
            { TypeIDs.STR, STR },
            { TypeIDs.IMG, IMG },
            { TypeIDs.IMG2, IMG },
            { TypeIDs.DIR, DIR },
            { TypeIDs.OBJD, OBJD },
            { TypeIDs.MP3, MP3 }
        };
        /// <summary>
        /// Get the codec for a filetype.
        /// </summary>
        /// <param name="type">The Type ID</param>
        /// <returns>Codec for this type.</returns>
        public static AbstractCodec Get(uint type)
        {
            return codecsByTypeID[type];
        }
    }
}

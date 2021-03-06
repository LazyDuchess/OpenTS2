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

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codecs for DBPF archives.
    /// </summary>
    public static class Codecs
    {
        static Dictionary<uint, AbstractCodec> codecsByTypeID = new Dictionary<uint, AbstractCodec>()
        {
            { Types.STR, new STRCodec() },
            { Types.IMG, new IMGCodec() },
            { Types.IMG2, new IMGCodec() }
        };

        public static AbstractCodec GetCodecInstanceForType(uint type)
        {
            return codecsByTypeID[type];
        }
    }
}

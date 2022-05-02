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

namespace OpenTS2.Files.Formats.DBPF
{

    /// <summary>
    /// STR file reading codec.
    /// </summary>
    public class STRCodec : AbstractCodec
    {

        /// <summary>
        /// Constructs a new STR instance.
        /// </summary>
        public STRCodec()
        {

        }

        /// <summary>
        /// Parses STR from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, TGI tgi, string sourceFile)
        {
            var stringTableData = new StringSetData();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            stringTableData.fileName = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 66);
            var stringSets = reader.ReadUInt16();
            for (var i = 0; i < stringSets; i++)
            {
                var languageCode = reader.ReadByte();
                var value = reader.ReadNullTerminatedUTF8();
                var desc = reader.ReadNullTerminatedUTF8();
                if (!stringTableData.strings.ContainsKey(languageCode))
                    stringTableData.strings[languageCode] = new List<StringValue>();
                stringTableData.strings[languageCode].Add(new StringValue(value, desc));
            }
            stream.Dispose();
            reader.Dispose();
            var ast = new StringSetAsset(stringTableData);
            return ast; 
        }
    }
}
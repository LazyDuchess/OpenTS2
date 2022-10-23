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
using OpenTS2.Common.Utils;
using OpenTS2.Client;

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
        [Codec(TypeIDs.STR)]
        public STRCodec()
        {

        }

        /// <summary>
        /// Parses STR from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stringTableData = new StringSetData();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            stringTableData.fileName = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 66);
            var stringSets = reader.ReadUInt16();
            for (var i = 0; i < stringSets; i++)
            {
                var languageCode = (Languages)reader.ReadByte();
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

        public class StringForSerialization
        {
            public Languages languageCode;
            public string value;
            public string description;
        }
        public override byte[] Serialize(AbstractAsset asset)
        {
            var stringAsset = asset as StringSetAsset;
            var stream = new MemoryStream(0);
            var writer = new BinaryWriter(stream);
            writer.Write(new byte[64]);
            writer.Seek(0, SeekOrigin.Begin);
            var ascii = Encoding.UTF8.GetBytes(stringAsset.StringData.fileName + char.MinValue);
            writer.Write(ascii);
            writer.Seek(64, SeekOrigin.Begin);
            writer.Write(new byte[2] { 253, 255 });
            var stringList = new List<StringForSerialization>();
            foreach(var element in stringAsset.StringData.strings)
            {
                foreach(var listElement in element.Value)
                {
                    var item = new StringForSerialization();
                    item.languageCode = element.Key;
                    item.value = listElement.value;
                    item.description = listElement.description;
                    stringList.Add(item);
                }
            }
            writer.Write((short)stringList.Count);
            foreach(var element in stringList)
            {
                writer.Write((byte)element.languageCode);
                ascii = Encoding.UTF8.GetBytes(element.value + char.MinValue);
                writer.Write(ascii);
                ascii = Encoding.UTF8.GetBytes(element.description + char.MinValue);
                writer.Write(ascii);
            }
            var buffer = StreamUtils.GetBuffer(stream);
            writer.Dispose();
            stream.Dispose();
            return buffer;
        }
    }
}
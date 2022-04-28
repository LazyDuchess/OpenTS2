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

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// A string set with a value and a description.
    /// </summary>
    public class StringSet
    {
        public string value;
        public string description;

        public StringSet(string value, string description)
        {
            this.value = value;
            this.description = description;
        }
    }
    /// <summary>
    /// STR file reading codec.
    /// </summary>
    public class STR : AbstractCodec
    {
        public string fileName;
        public Dictionary<byte, List<StringSet>> strings = new Dictionary<byte, List<StringSet>>();

        /// <summary>
        /// Gets a string by its ID and in the specified language.
        /// </summary>
        /// <param name="id">ID of the string to retrieve.</param>
        /// <param name="language">Language bytecode.</param>
        /// <returns>Localized string.</returns>
        public string GetString(int id, byte language)
        {
            return strings[language][id].value;
        }

        /// <summary>
        /// Constructs a new STR instance.
        /// </summary>
        public STR()
        {

        }

        /// <summary>
        /// Parses STR from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override void Deserialize(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            fileName = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 66);
            var stringSets = reader.ReadUInt16();
            for (var i = 0; i < stringSets; i++)
            {
                var languageCode = reader.ReadByte();
                var value = reader.ReadNullTerminatedUTF8();
                var desc = reader.ReadNullTerminatedUTF8();
                if (!strings.ContainsKey(languageCode))
                    strings[languageCode] = new List<StringSet>();
                strings[languageCode].Add(new StringSet(value, desc));
            }
            stream.Dispose();
            reader.Dispose();
        }

        public override AbstractAsset BuildAsset()
        {
            var asset = new StringTable(this);
            PostBuildAsset(asset);
            return asset;
        }
    }
}
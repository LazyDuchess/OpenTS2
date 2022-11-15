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
using OpenTS2.Common.Utils;

namespace OpenTS2.Files.Formats.DBPF
{

    /// <summary>
    /// DIR file reading codec.
    /// </summary>
    [Codec(TypeIDs.DIR)]
    public class DIRCodec : AbstractCodec
    {

        /// <summary>
        /// Constructs a new DIR instance.
        /// </summary>
        public DIRCodec()
        {

        }

        public override byte[] Serialize(AbstractAsset asset)
        {
            var dirAsset = asset as DIRAsset;
            var stream = new MemoryStream(0);
            var writer = new BinaryWriter(stream);
            foreach(var element in dirAsset.SizeByInternalTGI)
            {
                writer.Write(element.Key.TypeID);
                writer.Write(element.Key.GroupID);
                writer.Write(element.Key.InstanceID);
                writer.Write(element.Key.InstanceHigh);
                writer.Write(element.Value);
            }
            var buffer = StreamUtils.GetBuffer(stream);
            writer.Dispose();
            stream.Dispose();
            return buffer;
        }

        /// <summary>
        /// Parses DIR from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var dirAsset = new DIRAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            while (stream.Position < bytes.Length)
            {
                var TypeID = reader.ReadUInt32();
                var GroupID = reader.ReadUInt32();
                var InstanceID = reader.ReadUInt32();
                uint InstanceID2 = 0x0;
                if (sourceFile.IndexMinorVersion >= 2)
                    InstanceID2 = reader.ReadUInt32();
                dirAsset.SizeByInternalTGI[new ResourceKey(InstanceID, InstanceID2, GroupID, TypeID)] = reader.ReadUInt32();
            }
            reader.Dispose();
            stream.Dispose();
            return dirAsset;
        }
    }
}
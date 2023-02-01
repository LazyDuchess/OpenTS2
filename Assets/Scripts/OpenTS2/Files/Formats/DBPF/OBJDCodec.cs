using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codec for Object Definitions.
    /// </summary>
    [Codec(TypeIDs.OBJD)]
    public class OBJDCodec : AbstractCodec
    {
        //File Spec: https://modthesims.info/wiki.php?title=4F424A44

        /// <summary>
        /// Parses OBJD from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new ObjectDefinitionAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            asset.FileName = reader.ReadNullTerminatedUTF8();

            reader.Seek(SeekOrigin.Begin, 64);

            var assetType = typeof(ObjectDefinitionAsset);

            foreach (var field in ObjectDefinitionAsset.Fields)
            {
                var value = reader.ReadUInt16();
                if (field == "unused")
                    continue;
                assetType.GetProperty(field).SetValue(asset, value);
            }
            return asset;
        }
    }
}

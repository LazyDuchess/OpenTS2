using System;
using System.Collections.Generic;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.OBJ_SAVE_TYPE_TABLE)]
    public class ObjectSaveTypeTableCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip first 64 bytes.
            reader.Seek(SeekOrigin.Begin, 64);

            // Spells out "objt"
            var id = reader.ReadUInt32();
            if (id != TypeIDs.OBJ_SAVE_TYPE_TABLE)
            {
                throw new ArgumentException(
                    $"ObjectSaveTypeTable does not have correct ID: 0x{TypeIDs.OBJ_SAVE_TYPE_TABLE:X}, got: 0x{id:X}");
            }

            var version = reader.ReadUInt32();
            if (version < 0x49)
            {
                throw new ArgumentException($"ObjectSaveTypeTable version too old: {version}");
            }
            // Ignored uint32
            reader.ReadUInt32();

            // Next up is the selectors.
            var selectors = new List<ObjectSaveTypeTableAsset.ObjectSelector>();
            while (true)
            {
                var guid = reader.ReadUInt32();
                if (guid == 0)
                {
                    break;
                }

                var initTreeVersion = reader.ReadInt32();
                var mainTreeVersion = reader.ReadInt32();
                // Field 0x3A (maybe NumAttributes) from the object definition.
                var numAttributes = reader.ReadInt32();
                // Field 0x3B (maybe NumObjArrays) from the object definition.
                var numObjArrays = reader.ReadInt32();
                // The count of some sort of string set related to the object.
                var stringSetCount = reader.ReadInt32();
                var saveType = reader.ReadInt16();
                // Field 0x9 (maybe InteractionTableIDPointer) from the object definition.
                var interactionTableIDPointer = reader.ReadInt16();
                var catalogResourceName = reader.ReadVariableLengthPascalString();
                // Flags that seem to indicate whether this is a single tile object (bit 0)
                // and whether it is a "master" object (bit 1).
                var flags = reader.ReadUInt32();
                // Field 0xD0 (unknown) from the object definition.
                reader.ReadInt32();

                selectors.Add(new ObjectSaveTypeTableAsset.ObjectSelector(guid, saveType, catalogResourceName));
            }

            return new ObjectSaveTypeTableAsset(selectors);
        }
    }
}
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
    /// <summary>
    /// Codec for what is known in game as cTSObject. Contains attributes and locations of objects.
    /// </summary>
    [Codec(TypeIDs.XOBJ)]
    public class SimsObjectCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new SimsObjectAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip first 64 bytes.
            reader.Seek(SeekOrigin.Begin, 64);

            // TODO: this is for versions = 0xAD, see if we need to handle lower.

            // 4 skipped/unused floats
            for (int i = 0; i < 4; i++)
            {
                reader.ReadFloat();
            }

            var tileLocationY = reader.ReadFloat();
            var tileLocationX = reader.ReadFloat();
            var level = reader.ReadInt32();
            // ignored int16
            reader.ReadUInt16();
            var elevation = reader.ReadFloat();
            var objectGroupId = reader.ReadInt32();
            var unknown = reader.ReadInt16();

            Debug.Log($"tileLocationY: {tileLocationY}, tileLocationX: {tileLocationX}, level: {level}, " +
                      $"elevation: {elevation}, objectGroupId: {objectGroupId}, unknown: {unknown}");

            var numAttrs = reader.ReadInt16();
            var attrs = new short[numAttrs];
            for (var i = 0; i < numAttrs; i++)
            {
                attrs[i] = reader.ReadInt16();
            }
            Debug.Log($"numAttrs: {numAttrs}, attrs: [{string.Join(", ", attrs)}]");

            var numSemiAttrs = reader.ReadInt16();
            var semiAttrs = new short[numSemiAttrs];
            for (var i = 0; i < numSemiAttrs; i++)
            {
                semiAttrs[i] = reader.ReadInt16();
            }
            Debug.Log($"numSemiAttrs: {numAttrs}, semiAttrs: [{string.Join(", ", semiAttrs)}]");

            Debug.Log($"  Data array offset: {reader.Position:X}");

            /*
            // 8 unknown shorts called "data".
            var dataArray = new short[8];
            for (var i = 0; i < dataArray.Length; i++)
            {
                dataArray[i] = reader.ReadInt16();
            }
            Debug.Log($"dataArray: [{string.Join(", ", dataArray)}]");

            // Next is a number of shorts that depends on the exact version of the file.
            uint numShorts = 0x57 + 6;
            for (var i = 0; i < numShorts; i++)
            {
                reader.ReadInt16();
            }

            // Another 8 shorts, called the "tempTokenFields".
            for (var i = 0; i < 8; i++)
            {
                reader.ReadInt16();
            }

            // Next is the number of object arrays. Each being a short array itself.
            var numObjectArrays = reader.ReadInt16();
            var shortArrays = new List<short[]>(numObjectArrays);
            for (var i = 0; i < numObjectArrays; i++)
            {
                var objectArray = new short[reader.ReadInt16()];
                for (var j = 0; j < objectArray.Length; j++)
                {
                    objectArray[j] = reader.ReadInt16();
                }
                shortArrays.Add(objectArray);
            }
            Debug.Log($"numObjectArrays: {numObjectArrays}");

            // An array of shorts. Unknown.
            var numSecondShortArray = reader.ReadInt16();
            for (var i = 0; i < numSecondShortArray; i++)
            {
                reader.ReadInt16();
            }
            Debug.Log($"numSecondShortArrays: {numSecondShortArray}");

            var ownershipValue = reader.ReadInt32();
            Debug.Log($"ownershipValue: {ownershipValue}");

            Debug.Log($"  Position before strings: 0x{reader.Position:X}");
            // A number of material subsitution strings.
            var numMaterialSubstitues = reader.ReadInt16();
            Debug.Log($"  numMaterialSubstitues: {numMaterialSubstitues}");
            for (var i = 0; i < numMaterialSubstitues; i++)
            {
                var materialSubstitute = reader.ReadVariableLengthPascalString();
                Debug.Log($"materialSubstitute: {materialSubstitute}");
            }

            var persistentFlag = reader.ReadUInt16();
            Debug.Log($"persistentFlag: {persistentFlag}");

            // Slots...
            var slotsFlag = reader.ReadInt16();
            Debug.Log($"slotsFlag: {slotsFlag}");

            var numSlots = reader.ReadInt16();
            for (var i = 0; i < numSlots; i++)
            {
                var slotValue = reader.ReadInt16();
            }
            Debug.Log($"numSlots: {numSlots}");

            var numEffects = reader.ReadInt16();
            Debug.Log($"numEffects: {numEffects}");

            Debug.Log($"Position after numEffects: 0x{reader.Position:X}");

            var numbOverrides = reader.ReadInt16();
            Debug.Log($"numOverides: {numbOverrides}");

            for (int i = 0; i < numbOverrides; i++)
            {
                var overrideString1 = reader.ReadVariableLengthPascalString();
                var overrideString2 = reader.ReadVariableLengthPascalString();
                var overrideString3 = reader.ReadVariableLengthPascalString();
                Debug.Log($"{overrideString1} / {overrideString2} / {overrideString3}");
            }*/

            return asset;
        }
    }
}
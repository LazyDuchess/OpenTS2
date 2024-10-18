using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Codec for what is known in game as cTSObject. Contains state and locations of objects.
    /// </summary>
    [Codec(TypeIDs.XOBJ)]
    public class SimsObjectCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // This file needs a version that comes from the OBJM file. Without that, just default to 0xAD for now which
            // seems to be what my ultimate collection lots are saved with.
            return DeserializeWithVersion(reader, version: 0xAD);
        }

        private static SimsObjectAsset DeserializeWithVersion(IoBuffer reader, int version)
        {
            // Skip first 64 bytes.
            reader.Seek(SeekOrigin.Begin, 64);

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
            reader.ReadInt16(); // unknown

            var numAttrs = reader.ReadInt16();
            var attrs = new short[numAttrs];
            for (var i = 0; i < numAttrs; i++)
            {
                attrs[i] = reader.ReadInt16();
            }

            var numSemiAttrs = reader.ReadInt16();
            var semiAttrs = new short[numSemiAttrs];
            for (var i = 0; i < numSemiAttrs; i++)
            {
                semiAttrs[i] = reader.ReadInt16();
            }

            // 8 unknown shorts called "data".
            var dataArray = new short[8];
            for (var i = 0; i < dataArray.Length; i++)
            {
                dataArray[i] = reader.ReadInt16();
            }

            // Next is a number of shorts that depends on the exact version of the file.
            uint numShorts = version switch
            {
                0xAD => 0x58,
                _ => throw new NotImplementedException($"SimObjectCodec not implemented for version {version:X}"),
            };

            var temp = new short[8];
            for (var i = 0; i < temp.Length; i++)
            {
                temp[i] = reader.ReadInt16();
            }
            var data = new short[numShorts - 8];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadInt16();
            }

            // Another 8 shorts, called the "tempTokenFields".
            for (var i = 0; i < 8; i++)
            {
                reader.ReadInt16();
            }

            // Inventory token.
            var tokenGUID = reader.ReadUInt32();
            var tokenFlags = reader.ReadUInt16();
            var numTokenProperties = reader.ReadUInt32();
            for (var i = 0; i < numTokenProperties; i++)
            {
                reader.ReadUInt16();
            }
            Debug.Log($"InventoryToken(tokenGUID={tokenGUID}, tokenFlags={tokenFlags}, numTokenProps={numTokenProperties})");

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
            var materialSubstitutes = reader.ReadInt16();
            Debug.Log($"  numMaterialSubstitues: {materialSubstitutes}");
            for (var i = 0; i < materialSubstitutes; i++)
            {
                var materialSubstitute = reader.ReadVariableLengthPascalString();
                Debug.Log($"materialSubstitute: {materialSubstitute}");
            }

            var persistentFlag = reader.ReadUInt16();
            Debug.Log($"persistentFlag: {persistentFlag}");

            // Read the cTSTreeStack, a set of cTreeStackElems, probably the edith execution stack?
            var numStackFrames = reader.ReadInt32();
            var frames = new SimsObjectStackFrame[numStackFrames];
            reader.ReadUInt32(); // unknown
            for (var i = 0; i < numStackFrames; i++)
            {
                var objectID = reader.ReadUInt16();
                var treeID = reader.ReadUInt16();
                var nodeNum = reader.ReadUInt16();

                var numLocals = reader.ReadByte();
                Debug.Log($"- numLocals: {numLocals}");

                var numParams = reader.ReadByte();

                var runningObjId = reader.ReadUInt16();
                var runningOnObjId = reader.ReadUInt16();

                var frameParams = new short[numParams];
                for (var j = 0; j < frameParams.Length; j++)
                {
                    frameParams[j] = reader.ReadInt16();
                }

                var locals = new short[numLocals];
                for (var j = 0; j < locals.Length; j++)
                {
                    locals[j] = reader.ReadInt16();
                }

                var primState = reader.ReadInt32();

                // next part is related to loading the cITSBehavior
                var behavSaveType = reader.ReadUInt16();

                Debug.Log($"- objectID: {objectID}, bhav: {behavSaveType}, runningObjID: {runningObjId}, runningOnObjID: {runningOnObjId}");
                Debug.Log($"  locals: {string.Join(", ", locals)}");
                frames[i] = new SimsObjectStackFrame
                {
                    ObjectId = objectID,
                    TreeId = treeID,
                    BhavSaveType = behavSaveType,
                    Locals = locals,
                    Params = frameParams
                };
            }

            Debug.Log($"[P] Position before cTSRelationshipTable: 0x{reader.Position:X}");
            // Read the cTSRelationshipTable
            var relationshipTableFlag = reader.ReadInt32();
            Debug.Log($"  relationshipTableFlag: {relationshipTableFlag}");
            if (relationshipTableFlag < 0)
            {
                var relationShipCount = reader.ReadInt32();
                Debug.Log($"  relationShipCount: {relationShipCount}");
                for (var i = 0; i < relationShipCount; i++)
                {
                    var isPresent = reader.ReadInt32();
                    if (isPresent == 0)
                    {
                        continue;
                    }

                    var relationInstanceId = reader.ReadUInt32();

                    // TODO: read the entry here
                    var relationEntryCount = reader.ReadInt32();
                    for (var j = 0; j < relationEntryCount; j++)
                    {
                        var entry = reader.ReadUInt32();
                    }
                }
            } else if (relationshipTableFlag - 2 <= 1)
            {
                var relationShipCount = reader.ReadInt32();
                Debug.Log($"  relationShipCount: {relationShipCount}");
                for (var i = 0; i < relationShipCount; i++)
                {
                    var relationEntryId = reader.ReadUInt32();
                }
            }

            // Slots...
            var slotsFlag = reader.ReadInt16();
            Debug.Log($"slotsFlag: {slotsFlag}");
            if (slotsFlag < 0)
            {
                if (slotsFlag == -100)
                {
                    // Unknown short.
                    reader.ReadUInt16();
                }

                var numSlots = reader.ReadInt16();
                for (var i = 0; i < numSlots; i++)
                {
                    var slotValue = reader.ReadInt16();
                }
                Debug.Log($"numSlots: {numSlots}");
            }
            else
            {
                if (slotsFlag > 0)
                {
                    // Two unknown shorts.
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                }

                for (var i = 0; i < slotsFlag - 1; i++)
                {
                    // Two shorts per flag.
                    reader.ReadInt16();
                    reader.ReadInt16();
                }
            }

            Debug.Log($"Position before readEffects: 0x{reader.Position:X}");

            var effectsFlag = reader.ReadInt16();
            Debug.Log($"effectsFlag: {effectsFlag}");
            if (effectsFlag != 0)
            {
                var hasEffects = reader.ReadUInt16() == 1;
                Debug.Log($"hasEffects: {hasEffects}");
                if (hasEffects)
                {
                    var effectCount = reader.ReadUInt32();
                    for (var i = 0; i < effectCount; i++)
                    {
                        reader.ReadVariableLengthPascalString();
                        reader.ReadVariableLengthPascalString();
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                    }
                }
            }

            var numbOverrides = reader.ReadInt16();
            Debug.Log($"numOverides: {numbOverrides}");

            for (int i = 0; i < numbOverrides; i++)
            {
                var overrideString1 = reader.ReadVariableLengthPascalString();
                var overrideString2 = reader.ReadVariableLengthPascalString();
                var overrideString3 = reader.ReadVariableLengthPascalString();
                Debug.Log($"{overrideString1} / {overrideString2} / {overrideString3}");
            }

            return new SimsObjectAsset(tileLocationY: tileLocationY, tileLocationX: tileLocationX, level: level,
                elevation: elevation, objectGroupId: objectGroupId, attrs: attrs, semiAttrs: semiAttrs, temp: temp,
                data: data,
                stackFrames: frames);
        }
    }
}
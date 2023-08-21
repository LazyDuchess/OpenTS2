using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.SimAntics
{
    [Codec(TypeIDs.BHAV)]
    public class BHAVCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new BHAVAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Header
            asset.FileName = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 64);
            var magic = reader.ReadUInt16();
            Debug.Assert(magic <= 0x8009);
            var instructionCount = reader.ReadUInt16();
            var type = reader.ReadByte();
            var args = reader.ReadByte();
            var locals = reader.ReadByte();
            var flags = reader.ReadByte();
            var treeVersion = reader.ReadUInt32();

            // Nodes
            for(var i=0;i<instructionCount;i++)
            {
                ushort opcode, trueTarget, falseTarget;
                byte[] operands;

                if (magic <= 0x8002)
                {
                    opcode = reader.ReadUInt16();
                    trueTarget = reader.ReadByte();
                    falseTarget = reader.ReadByte();
                    operands = reader.ReadBytes(8);
                }
                else if (magic <= 0x8004)
                {
                    opcode = reader.ReadUInt16();
                    trueTarget = reader.ReadByte();
                    falseTarget = reader.ReadByte();
                    operands = reader.ReadBytes(16);
                }
                else if (magic <= 0x8006)
                {
                    opcode = reader.ReadUInt16();
                    trueTarget = reader.ReadByte();
                    falseTarget = reader.ReadByte();
                    var nodeVersion = reader.ReadByte();
                    operands = reader.ReadBytes(16);
                }
                else if (magic <= 0x8008)
                {
                    opcode = reader.ReadUInt16();
                    trueTarget = reader.ReadUInt16();
                    falseTarget = reader.ReadUInt16();
                    var nodeVersion = reader.ReadByte();
                    operands = reader.ReadBytes(16);
                }
                else
                {
                    opcode = reader.ReadUInt16();
                    trueTarget = reader.ReadUInt16();
                    falseTarget = reader.ReadUInt16();
                    var nodeVersion = reader.ReadByte();
                    operands = reader.ReadBytes(16);
                    var cacheFlags = reader.ReadByte();
                }

                // Convert from TS1 to TS2 format if necessary.
                trueTarget = ParseTarget(trueTarget);
                falseTarget = ParseTarget(falseTarget);

                var node = new BHAVAsset.Node
                {
                    OpCode = opcode,
                    TrueTarget = trueTarget,
                    FalseTarget = falseTarget,
                    Operands = operands
                };

                asset.Nodes.Add(node);
            }

            stream.Dispose();
            reader.Dispose();
            return asset;

            ushort ParseTarget(ushort target)
            {
                switch(target)
                {
                    // None / Error
                    case 0xFD:
                        return BHAVAsset.Node.ErrorReturnValue;
                    // True
                    case 0xFE:
                        return BHAVAsset.Node.TrueReturnValue;
                    // False
                    case 0xFF:
                        return BHAVAsset.Node.FalseReturnValue;
                    // probably already an okay value
                    default:
                        return target;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// Script in the SimAntics language. Handles most of the simulation.
    /// </summary>
    public class BHAVAsset : AbstractAsset
    {
        public string FileName = "";
        public int ArgumentCount = 0;
        public int LocalCount = 0;
        public List<Node> Nodes = new List<Node>();

        public class Node
        {
            public const ushort ErrorReturnValue = 0xFFFC;
            public const ushort TrueReturnValue = 0xFFFD;
            public const ushort FalseReturnValue = 0xFFFE;
            public ushort OpCode;
            public ushort TrueTarget;
            public ushort FalseTarget;
            public byte[] Operands;
            public byte Version;

            /// <summary>
            /// Retrieves the operand at index.
            /// </summary>
            /// <returns>Operand byte value, or 0 if the index is out of range.</returns>
            public byte GetOperand(int index)
            {
                if (index < 0)
                    return 0;
                if (index >= Operands.Length)
                    return 0;
                return Operands[index];
            }

            /// <summary>
            /// Retrieves an operand byte array starting at index and of the specified length.
            /// </summary>
            /// <returns>Operand bytes, bytes out of range return 0.</returns>
            public byte[] GetOperands(int index, int length)
            {
                var array = new byte[length];
                for(var i=0;i<length;i++)
                {
                    var ind = index + i;
                    array[i] = GetOperand(ind);
                }
                return array;
            }

            /// <summary>
            /// Retrieves an ushort from the operand array, starting at index.
            /// </summary>
            public ushort GetUInt16Operand(int index)
            {
                return BitConverter.ToUInt16(GetOperands(index,2), 0);
            }

            /// <summary>
            /// Retrieves a short from the operand array, starting at index.
            /// </summary>
            public short GetInt16Operand(int index)
            {
                return BitConverter.ToInt16(GetOperands(index, 2), 0);
            }
        }
    }
}

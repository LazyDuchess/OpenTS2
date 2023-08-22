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

            public byte GetOperand(int index)
            {
                if (index < 0)
                    return 0;
                if (index >= Operands.Length)
                    return 0;
                return Operands[index];
            }

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

            public ushort GetUInt16(int operand)
            {
                return BitConverter.ToUInt16(GetOperands(operand,2), 0);
            }

            public short GetInt16(int operand)
            {
                return BitConverter.ToInt16(GetOperands(operand, 2), 0);
            }
        }
    }
}

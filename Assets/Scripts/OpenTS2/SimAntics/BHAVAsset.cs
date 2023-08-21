using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;

namespace OpenTS2.SimAntics
{
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
        }
    }
}

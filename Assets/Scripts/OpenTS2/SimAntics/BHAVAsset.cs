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
        public List<Node> Nodes = new List<Node>();

        public class Node
        {
            public ushort OpCode;
            public ushort TrueTarget;
            public ushort FalseTarget;
            public byte[] Operands;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMStackFrame
    {
        public BHAVAsset BHAV;
        public ushort StackObjectID = 0;
        public int CurrentNode = 0;
        public VMReturnValue Tick()
        {
            return VMReturnValue.ReturnTrue;
        }
        public BHAVAsset.Node GetCurrentNode()
        {
            return BHAV.Nodes[CurrentNode];
        }
    }
}

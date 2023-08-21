using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMStackFrame
    {
        public VMStack Stack;
        public BHAVAsset BHAV;
        public ushort StackObjectID = 0;
        public int CurrentNode = 0;
        public Func<VMReturnValue> CurrentContinueCallback = null;
        public short[] Locals;
        public short[] Arguments;
        public VMStackFrame(BHAVAsset bhav, VMStack stack)
        {
            BHAV = bhav;
            Stack = stack;
            Locals = new short[BHAV.LocalCount];
            Arguments = new short[BHAV.ArgumentCount];
        }
        public VMReturnValue Tick()
        {
            if (CurrentContinueCallback != null)
                return CurrentContinueCallback.Invoke();
            return VMReturnValue.ReturnTrue;
        }
        public void SetCurrentNode(int nodeIndex)
        {
            CurrentNode = nodeIndex;
            CurrentContinueCallback = null;
        }
        public BHAVAsset.Node GetCurrentNode()
        {
            return BHAV.Nodes[CurrentNode];
        }
    }
}

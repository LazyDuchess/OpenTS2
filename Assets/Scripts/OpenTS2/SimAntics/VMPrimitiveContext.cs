using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public struct VMPrimitiveContext
    {
        public VMStackFrame StackFrame;
        public BHAVAsset.Node Node;
        public VMStack Stack => StackFrame.Stack;
        public VMEntity Entity => StackFrame.Stack.Entity;
        public VM VM => Entity.VM;
    }
}

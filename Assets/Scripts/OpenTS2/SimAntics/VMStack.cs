using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMStack
    {
        // For check trees and other things that should execute and return immediately we should set this to false.
        public bool CanYield = true;
        public VMEntity Entity;
        public Stack<VMStackFrame> Frames = new Stack<VMStackFrame>();
        public VMStack(VMEntity entity)
        {
            Entity = entity;
        }
        public VMReturnValue.ExitCode Tick()
        {
            if (Frames.Count == 0)
                return VMReturnValue.ExitCode.False;
            var currentFrame = Frames.Peek();
            var returnValue = currentFrame.Tick();
            if (returnValue.Code == VMReturnValue.ExitCode.Continue && !CanYield)
                throw new Exception("Attempted to yield in a non-yielding VMStack.");
            while (returnValue.Code != VMReturnValue.ExitCode.Continue)
            {
                Frames.Pop();
                if (Frames.Count == 0)
                    return returnValue.Code;
                currentFrame = Frames.Peek();
                var currentNode = currentFrame.GetCurrentNode();

                if (returnValue.Code == VMReturnValue.ExitCode.True)
                    currentFrame.CurrentNode = currentNode.TrueTarget;
                else
                    currentFrame.CurrentNode = currentNode.FalseTarget;

                returnValue = currentFrame.Tick();
            }
            return returnValue.Code;
        }
    }
}

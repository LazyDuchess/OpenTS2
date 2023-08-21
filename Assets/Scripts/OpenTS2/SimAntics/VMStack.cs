using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMStack
    {
        public VMEntity Entity;
        public Stack<VMStackFrame> Frames = new Stack<VMStackFrame>();
        public VMStack(VMEntity entity)
        {
            Entity = entity;
        }
        public void Tick()
        {
            var currentFrame = Frames.Peek();
            if (currentFrame == null)
                return;
            var returnValue = currentFrame.Tick();
            while (returnValue.Code != VMReturnValue.ExitCode.Continue)
            {
                Frames.Pop();
                currentFrame = Frames.Peek();
                if (currentFrame == null)
                    return;
                var currentNode = currentFrame.GetCurrentNode();

                if (returnValue.Code == VMReturnValue.ExitCode.GoToTrue)
                    currentFrame.CurrentNode = currentNode.TrueTarget;
                else
                    currentFrame.CurrentNode = currentNode.FalseTarget;

                returnValue = currentFrame.Tick();
            }
        }
    }
}

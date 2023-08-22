using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// Stack of scripts to run on a SimAntics entity/thread.
    /// </summary>
    public class VMStack
    {
        public bool Interrupted => _interrupt == true && Entity.VM.CurrentTick >= _interruptTick;
        private bool _interrupt = false;
        private uint _interruptTick = 0;
        // For check trees and other things that should execute and return immediately we should set this to false.
        public bool CanYield = true;
        public VMEntity Entity;
        public Stack<VMStackFrame> Frames = new Stack<VMStackFrame>();
        public VMStack(VMEntity entity)
        {
            Entity = entity;
        }

        public VMStackFrame GetCurrentFrame()
        {
            if (Frames.Count == 0)
                return null;
            return Frames.Peek();
        }

        public VMExitCode Tick()
        {
            var currentFrame = GetCurrentFrame();
            if (currentFrame == null)
                return VMExitCode.False;
            var returnValue = currentFrame.Tick();
            if (returnValue == VMExitCode.Continue && !CanYield)
                throw new Exception("Attempted to yield in a non-yielding VMStack.");
            while (returnValue != VMExitCode.Continue)
            {
                Frames.Pop();
                currentFrame = GetCurrentFrame();
                if (currentFrame == null)
                    return returnValue;
                var currentNode = currentFrame.GetCurrentNode();

                if (returnValue == VMExitCode.True)
                    currentFrame.CurrentNode = currentNode.TrueTarget;
                else
                    currentFrame.CurrentNode = currentNode.FalseTarget;

                returnValue = currentFrame.Tick();
            }
            HandleInterrupt();
            return returnValue;
        }

        void HandleInterrupt()
        {
            _interrupt = false;
            _interruptTick = 0;
        }

        public void Interrupt()
        {
            if (_interrupt)
                return;
            _interrupt = true;
            _interruptTick = Entity.VM.CurrentTick + 1;
        }
    }
}

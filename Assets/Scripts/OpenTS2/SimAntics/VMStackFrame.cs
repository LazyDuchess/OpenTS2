using OpenTS2.SimAntics.Primitives;
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
        public Func<VMReturnValue.ExitCode> CurrentContinueCallback = null;
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
            {
                var primReturn = new VMReturnValue { Code = CurrentContinueCallback.Invoke() };
                if (primReturn.Code == VMReturnValue.ExitCode.Continue)
                    return primReturn;
                else
                {
                    return AdvanceFrame(primReturn);
                }
            }
            return RunCurrentFrame();
        }

        VMReturnValue AdvanceFrame(VMReturnValue returnValue)
        {
            var currentNode = GetCurrentNode();
            ushort returnTarget = 0;
            if (returnValue.Code == VMReturnValue.ExitCode.True)
                returnTarget = currentNode.TrueTarget;
            else
                returnTarget = currentNode.FalseTarget;
            switch (returnTarget)
            {
                case BHAVAsset.Node.FalseReturnValue:
                    return VMReturnValue.ReturnFalse;
                case BHAVAsset.Node.TrueReturnValue:
                    return VMReturnValue.ReturnTrue;
                case BHAVAsset.Node.ErrorReturnValue:
                    throw new Exception("Jumped to Error.");
                default:
                    SetCurrentNode(returnTarget);
                    return RunCurrentFrame();
            }
        }

        VMReturnValue RunCurrentFrame()
        {
            var currentNode = GetCurrentNode();
            if (currentNode != null)
            {
                var context = new VMPrimitiveContext
                {
                    StackFrame = this,
                    Node = currentNode
                };
                var opcode = currentNode.OpCode;
                var prim = VMPrimitiveRegistry.GetPrimitive(opcode);
                if (prim != null)
                {
                    var primReturn = prim.Execute(context);

                    if (primReturn.Code == VMReturnValue.ExitCode.Continue)
                        primReturn.Code = primReturn.ContinueCallback.Invoke();

                    if (primReturn.Code == VMReturnValue.ExitCode.Continue)
                    {
                        CurrentContinueCallback = primReturn.ContinueCallback;
                        return primReturn;
                    }
                    else
                    {
                        CurrentContinueCallback = null;
                        ushort returnTarget = 0;
                        if (primReturn.Code == VMReturnValue.ExitCode.True)
                            returnTarget = currentNode.TrueTarget;
                        else
                            returnTarget = currentNode.FalseTarget;
                        switch(returnTarget)
                        {
                            case BHAVAsset.Node.FalseReturnValue:
                                return VMReturnValue.ReturnFalse;
                            case BHAVAsset.Node.TrueReturnValue:
                                return VMReturnValue.ReturnTrue;
                            case BHAVAsset.Node.ErrorReturnValue:
                                throw new Exception("Jumped to Error.");
                            default:
                                SetCurrentNode(returnTarget);
                                return RunCurrentFrame();
                        }
                    }
                }
            }
            return VMReturnValue.ReturnFalse;
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

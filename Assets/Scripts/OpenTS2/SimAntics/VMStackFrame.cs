using OpenTS2.Files.Formats.DBPF;
using OpenTS2.SimAntics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// A frame in a SimAntics stack.
    /// </summary>
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
                    return AdvanceNodeAndRunTick(primReturn);
                }
            }
            return RunCurrentTick();
        }

        VMReturnValue AdvanceNodeAndRunTick(VMReturnValue returnValue)
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
                    return RunCurrentTick();
            }
        }

        VMReturnValue RunCurrentTick()
        {
            var currentNode = GetCurrentNode();
            if (currentNode != null)
            {
                var context = new VMContext
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
                        return AdvanceNodeAndRunTick(primReturn);
                    }
                }
                else
                {
                    var newStackFrame = CreateStackFrameForNode(context);
                    if (newStackFrame != null)
                    {
                        Stack.Frames.Push(newStackFrame);
                        return newStackFrame.Tick();
                    }
                }
            }
            return VMReturnValue.ReturnFalse;
        }

        enum GoSubFormat
        {
            PassTemps,
            TS1,
            TS2,
            CallerParams
        }

        VMStackFrame CreateStackFrameForNode(VMContext ctx)
        {
            var bhav = GetBHAVForOpCode(ctx.Node.OpCode);

            if (bhav == null)
                return null;

            var newStackFrame = new VMStackFrame(bhav, Stack);
            newStackFrame.StackObjectID = ctx.StackFrame.StackObjectID;

            GoSubFormat format = GoSubFormat.PassTemps;

            if (ctx.Node.GetOperand(12) > 0)
            {
                format = GoSubFormat.TS2;
                if (ctx.Node.GetOperand(12) == 2 && ctx.Node.Version > 0)
                    format = GoSubFormat.CallerParams;
            }
            else
            {
                for (var i = 0; i < 8; i++)
                {
                    if (ctx.Node.Operands[i] != 0xFF)
                    {
                        format = GoSubFormat.TS1;
                        break;
                    }
                }
            }

            var argAmount = 0;

            switch(format)
            {
                case GoSubFormat.PassTemps:
                    argAmount = Math.Min(newStackFrame.Arguments.Length, Stack.Entity.Temps.Length);
                    for (var i=0;i<argAmount;i++)
                    {
                        newStackFrame.TrySetArgument(i, Stack.Entity.Temps[i]);
                    }
                    break;
                case GoSubFormat.TS1:
                    argAmount = Math.Min(newStackFrame.Arguments.Length, 8);
                    for(var i=0;i<argAmount;i++)
                    {
                        newStackFrame.TrySetArgument(i, ctx.Node.GetInt16(i*2));
                    }
                    break;
                case GoSubFormat.TS2:
                    argAmount = Math.Min(newStackFrame.Arguments.Length, 4);
                    for(var i=0;i<argAmount;i++)
                    {
                        var dataSourceIndex = i * 3;
                        var dataValueIndex = dataSourceIndex + 1;

                        var dataSource = (VMDataSource)ctx.Node.GetOperand(dataSourceIndex);
                        var dataValue = ctx.Node.GetUInt16(dataValueIndex);

                        var data = ctx.GetData(dataSource, dataValue);

                        newStackFrame.TrySetArgument(i, data);
                    }
                    break;
                case GoSubFormat.CallerParams:
                    argAmount = Math.Min(newStackFrame.Arguments.Length, ctx.StackFrame.Arguments.Length);
                    for (var i=0;i<argAmount;i++)
                    {
                        newStackFrame.TrySetArgument(i, ctx.StackFrame.Arguments[i]);
                    }
                    break;
            }

            return newStackFrame;
        }

        void TrySetArgument(int index, short value)
        {
            if (index < 0)
                return;
            if (Arguments.Length <= index)
                return;
            Arguments[index] = value;
        }

        BHAVAsset GetBHAVForOpCode(ushort opCode)
        {
            if (opCode < 0x1000)
            {
                return VM.GetBHAV(opCode, GroupIDs.Global);
            }
            else if (opCode < 0x2000)
            {
                return VM.GetBHAV(opCode, BHAV.GlobalTGI.GroupID);
            }
            else
            {
                // TODO: Return Semi-Global BHAVs here.
                return null;
            }
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

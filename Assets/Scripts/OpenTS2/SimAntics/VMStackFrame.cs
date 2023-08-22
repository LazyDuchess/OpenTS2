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
        public short StackObjectID = 0;
        public int CurrentNode = 0;
        /// <summary>
        /// Current blocking behavior. As long as this variable's Tick() returns Continue this thread won't move.
        /// </summary>
        public VMContinueHandler CurrentContinueHandler = null;
        public short[] Locals;
        public short[] Arguments;

        // TODO - Return proper Group IDs. Private should come from the entity's OBJD, semiglobal from the entity's global resource.
        private uint PrivateGroupID => BHAV.GlobalTGI.GroupID;
        private uint SemiGlobalGroupID => 0x0;
        private uint GlobalGroupID => GroupIDs.Global;

        public VMStackFrame(BHAVAsset bhav, VMStack stack)
        {
            BHAV = bhav;
            Stack = stack;
            Locals = new short[BHAV.LocalCount];
            Arguments = new short[BHAV.ArgumentCount];
        }

        public VMExitCode Tick()
        {
            if (CurrentContinueHandler != null)
            {
                var returnCode = CurrentContinueHandler.Tick();
                if (returnCode == VMExitCode.Continue)
                    return returnCode;
                else
                {
                    return AdvanceNodeAndRunTick(returnCode);
                }
            }
            return RunCurrentTick();
        }

        VMExitCode AdvanceNodeAndRunTick(VMExitCode exitCode)
        {
            var currentNode = GetCurrentNode();
            ushort returnTarget = 0;
            if (exitCode == VMExitCode.True)
                returnTarget = currentNode.TrueTarget;
            else
                returnTarget = currentNode.FalseTarget;
            switch (returnTarget)
            {
                case BHAVAsset.Node.FalseReturnValue:
                    return VMExitCode.False;
                case BHAVAsset.Node.TrueReturnValue:
                    return VMExitCode.True;
                case BHAVAsset.Node.ErrorReturnValue:
                    throw new Exception("Jumped to Error.");
                default:
                    SetCurrentNode(returnTarget);
                    return RunCurrentTick();
            }
        }

        VMExitCode RunCurrentTick()
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

                    if (primReturn.Code == VMExitCode.Continue)
                        primReturn.Code = primReturn.ContinueHandler.Tick();

                    if (primReturn.Code == VMExitCode.Continue)
                    {
                        CurrentContinueHandler = primReturn.ContinueHandler;
                        return primReturn.Code;
                    }
                    else
                    {
                        CurrentContinueHandler = null;
                        return AdvanceNodeAndRunTick(primReturn.Code);
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
            return VMExitCode.False;
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
            // 0x0XXX is global scope, 0x1XXX is private scope and 0x2XXX is semiglobal scope.
            var groupid = SemiGlobalGroupID;

            if (opCode < 0x1000)
                groupid = GlobalGroupID;
            else if (opCode < 0x2000)
                groupid = PrivateGroupID;

            return VM.GetBHAV(opCode, groupid);
        }

        public void SetCurrentNode(int nodeIndex)
        {
            CurrentNode = nodeIndex;
            CurrentContinueHandler = null;
        }
        public BHAVAsset.Node GetCurrentNode()
        {
            return BHAV.Nodes[CurrentNode];
        }
    }
}

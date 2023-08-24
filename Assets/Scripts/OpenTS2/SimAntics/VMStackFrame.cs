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
        private static int MaxIterations = 500000;

        public VMStackFrame(BHAVAsset bhav, VMStack stack)
        {
            BHAV = bhav;
            Stack = stack;
            Locals = new short[BHAV.LocalCount];
            Arguments = new short[BHAV.ArgumentCount];
        }

        public VMExitCode Tick()
        {
            // This can probably be cleaned up, had to make some changes to fix the stack overflowing cause i'm kinda dumb.

            var currentIterations = 0;
            var nodeExecuted = false;
            VMExitCode result = VMExitCode.Continue;

            if (CurrentContinueHandler != null)
            {
                result = CurrentContinueHandler.Tick();
                if (result == VMExitCode.Continue)
                    return result;
                // This tells the following code to just transition to the next node, as we already ran this node.
                nodeExecuted = true;
            }

            var currentNode = GetCurrentNode();
            if (!nodeExecuted)
                result = ExecuteNode(currentNode);
            if (result == VMExitCode.Continue)
                return result;
            var returnTarget = GetNodeReturnTarget(currentNode, result);

            while (returnTarget != BHAVAsset.Node.ErrorReturnValue && returnTarget != BHAVAsset.Node.TrueReturnValue && returnTarget != BHAVAsset.Node.FalseReturnValue)
            {
                currentIterations++;
                if (currentIterations > MaxIterations && MaxIterations > 0)
                {
                    throw new SimAnticsException($"Thread entered infinite loop! ( >{MaxIterations} primitives )", this);
                }
                SetCurrentNode(returnTarget);
                currentNode = GetCurrentNode();
                result = ExecuteNode(currentNode);
                if (result == VMExitCode.Continue)
                    return result;
                returnTarget = GetNodeReturnTarget(currentNode, result);
            }

            if (returnTarget == BHAVAsset.Node.ErrorReturnValue)
                throw new SimAnticsException("Node transitioned to Error.", this);

            if (returnTarget == BHAVAsset.Node.TrueReturnValue)
                return VMExitCode.True;

            return VMExitCode.False;
        }

        ushort GetNodeReturnTarget(BHAVAsset.Node node, VMExitCode exitCode)
        {
            ushort returnTarget;
            if (exitCode == VMExitCode.True)
                returnTarget = node.TrueTarget;
            else
                returnTarget = node.FalseTarget;
            return returnTarget;
        }

        VMExitCode ExecuteNode(BHAVAsset.Node node)
        {
            if (node != null)
            {
                var context = new VMContext
                {
                    StackFrame = this,
                    Node = node
                };
                var opcode = node.OpCode;
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
                        return primReturn.Code;
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

        // Creates a new stack frame, to push onto the stack to run other scripts.
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

            int argAmount;

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
                        newStackFrame.TrySetArgument(i, ctx.Node.GetInt16Operand(i*2));
                    }
                    break;
                case GoSubFormat.TS2:
                    argAmount = Math.Min(newStackFrame.Arguments.Length, 4);
                    for(var i=0;i<argAmount;i++)
                    {
                        var dataSourceIndex = i * 3;
                        var dataValueIndex = dataSourceIndex + 1;

                        var dataSource = (VMDataSource)ctx.Node.GetOperand(dataSourceIndex);
                        var dataValue = ctx.Node.GetInt16Operand(dataValueIndex);

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
            var groupid = Stack.Entity.SemiGlobalGroupID;

            if (opCode < 0x1000)
                groupid = GroupIDs.Global;
            else if (opCode < 0x2000)
                groupid = Stack.Entity.PrivateGroupID;

            return VM.GetBHAV(opCode, groupid);
        }

        public void SetCurrentNode(int nodeIndex)
        {
            if (nodeIndex > BHAV.Nodes.Count || nodeIndex < 0)
            {
                throw new SimAnticsException("Attempted to transition to node out of range.", this);
            }
            CurrentNode = nodeIndex;
            CurrentContinueHandler = null;
        }

        public BHAVAsset.Node GetCurrentNode()
        {
            return BHAV.Nodes[CurrentNode];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    /// <summary>
    /// Sleeps the current thread for a number of ticks.
    /// </summary>
    public class VMSleep : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var argumentIndex = ctx.Node.GetUInt16Operand(0);
            var sleepTicks = (uint)Math.Max(0,(int)ctx.StackFrame.Arguments[argumentIndex]);
            return new VMReturnValue(new ContinueHandler(ctx.Stack, sleepTicks));
        }

        /// <summary>
        /// Handles VM thread blocking for the Sleep prim.
        /// </summary>
        public class ContinueHandler : VMContinueHandler
        {
            public uint TargetTick = 0;
            VMStack _stack;

            public ContinueHandler(VMStack stack, uint ticks)
            {
                _stack = stack;
                var vm = _stack.Entity.VM;
                TargetTick = vm.CurrentTick + ticks;
            }

            public override VMExitCode Tick()
            {
                if (_stack.Interrupt)
                {
                    // Handled!
                    _stack.Interrupt = false;
                    return VMExitCode.True;
                }
                if (_stack.Entity.VM.CurrentTick >= TargetTick)
                    return VMExitCode.True;
                return VMExitCode.Continue;
            }
        }
    }
}

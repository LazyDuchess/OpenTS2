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
            return new VMReturnValue(new ContinueHandler(ctx.Thread, sleepTicks));
        }

        /// <summary>
        /// Handles VM thread blocking for the Sleep prim.
        /// </summary>
        public class ContinueHandler : VMContinueHandler
        {
            public uint TargetTick = 0;
            VMThread _thread;

            public ContinueHandler(VMThread thread, uint ticks)
            {
                _thread = thread;
                var vm = _thread.Entity.VM;
                TargetTick = vm.CurrentTick + ticks;
            }

            public override VMExitCode Tick()
            {
                if (_thread.Interrupt)
                {
                    // Handled!
                    _thread.Interrupt = false;
                    return VMExitCode.True;
                }
                if (_thread.Entity.VM.CurrentTick >= TargetTick)
                    return VMExitCode.True;
                return VMExitCode.Continue;
            }
        }
    }
}

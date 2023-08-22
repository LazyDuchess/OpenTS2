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
    public class VMSleepPrimitive : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var argumentIndex = ctx.Node.GetUInt16(0);
            var sleepTicks = ctx.StackFrame.Arguments[argumentIndex];
            return VMReturnValue.Sleep(ctx.VM, (uint)sleepTicks);
        }
    }
}

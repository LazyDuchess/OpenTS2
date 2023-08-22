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
            var argumentIndex = ctx.Node.GetUInt16(0);
            var sleepTicks = (uint)Math.Max(0,(int)ctx.StackFrame.Arguments[argumentIndex]);
            return new VMReturnValue(new VMSleepContinueHandler(ctx.Stack, sleepTicks));
        }
    }
}

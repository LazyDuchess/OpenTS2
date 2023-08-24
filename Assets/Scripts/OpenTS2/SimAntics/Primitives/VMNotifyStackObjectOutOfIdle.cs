using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    public class VMNotifyStackObjectOutOfIdle : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var stackObject = ctx.VM.GetEntityByID(ctx.StackFrame.StackObjectID);
            if (stackObject == null)
                throw new SimAnticsException($"Couldn't find Object with ID {ctx.StackFrame.StackObjectID}", ctx.StackFrame);
            ctx.VM.Scheduler.ScheduleInterrupt(ctx.StackObjectEntity.Stack);
            return VMReturnValue.ReturnTrue;
        }
    }
}

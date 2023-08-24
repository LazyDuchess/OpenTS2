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
            var stackObj = ctx.StackObjectEntity;
            if (stackObj != null)
                ctx.VM.Scheduler.ScheduleInterrupt(stackObj.Stack);
            return VMReturnValue.ReturnTrue;
        }
    }
}

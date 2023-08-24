using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;

namespace OpenTS2.SimAntics.Primitives
{
    public class VMRemoveObjectInstance : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            // These two seem to be a mystery according to FreeSO src
            var returnImmediately = ((ctx.Node.GetOperand(2) & 1) == 1);
            var cleanUpAll = ((ctx.Node.GetOperand(2) & 2) != 2);

            var entityToRemove = ctx.Node.GetOperand(0) > 0 ? ctx.StackObjectEntity : ctx.Entity;
            entityToRemove?.Delete();

            // FreeSO yields 1 tick in the case of self deletion, preventing further execution of the script, so we also do that.
            // TS2 BHAVs tend to remove themselves -> idle for a few ticks -> loop back to remove prim.
            if (entityToRemove == ctx.Entity)
                return VMReturnValue.ReturnTrueNextTick;

            return VMReturnValue.ReturnTrue;
        }
    }
}

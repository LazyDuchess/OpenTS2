using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public abstract class VMPrimitive
    {
        public abstract VMReturnValue Execute(VMPrimitiveContext ctx);
    }
}

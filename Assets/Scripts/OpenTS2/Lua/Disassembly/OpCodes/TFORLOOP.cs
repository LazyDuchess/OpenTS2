using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class TFORLOOP : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var start = A + 2;
            var end = A + 2 + C;
            var loopValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    loopValues += ", ";
                loopValues += context.R((ushort)i);

                // Probably don't need to do this
                context.Code.WriteLine(context.R((ushort)i) + " = nil");
            }

            context.Code.WriteLine("for " + loopValues + " in " + context.R((ushort)(A)) + ", " + context.R((ushort)(A + 1)) + " do");
        }
    }
}

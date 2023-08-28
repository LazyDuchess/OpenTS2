using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class SETLIST : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var elementAmount = Bx % FPF + 1;
            for (var i = 0; i < elementAmount; i++)
            {
                var index = (ushort)(i + 1);
                context.Code.WriteLine(context.R(A) + "[" + (Bx - Bx % FPF + index).ToString() + "] = " + context.R((ushort)(A + index)));
            }
        }
    }
}

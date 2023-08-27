using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class GETTABLE : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            context.Code.WriteLine($"{context.R(A)} = {context.R(B)}[{context.RKAsString(C)}]");
        }
    }
}

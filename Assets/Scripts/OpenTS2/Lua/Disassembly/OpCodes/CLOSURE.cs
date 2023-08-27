using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class CLOSURE : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var func = context.KProto(Bx);
            func.DisassembleIntoContext(context);
        }
    }
}

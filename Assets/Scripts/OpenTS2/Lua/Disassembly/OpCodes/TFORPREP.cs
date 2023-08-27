using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class TFORPREP : LuaC50.OpCode
    {
        public override void PreProcess(LuaC50.Context context)
        {
            context.MakeRelativeJump(sBx + 1);
        }
        public override void Disassemble(LuaC50.Context context)
        {
            context.Code.WriteLine(context.R((ushort)(A + 1)) + " = " + context.R(A)+ " -- tforprep...");
            var jmp = context.MakeRelativeJump(sBx + 1);
            context.Code.WriteGoto(jmp);
        }
    }
}

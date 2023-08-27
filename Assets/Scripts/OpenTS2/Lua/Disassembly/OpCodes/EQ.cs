using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class EQ : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            if (GetBool(A))
                context.Code.WriteLine($"if ({context.RKAsString(B)} ~= {context.RKAsString(C)}) then");
            else
                context.Code.WriteLine($"if ({context.RKAsString(B)} == {context.RKAsString(C)}) then");
            context.Code.Indentation++;
            var targetLabel = context.MakeRelativeJump(2);
            context.Code.WriteGoto(targetLabel);
            context.Code.Indentation--;
            context.Code.WriteEnd();
        }
    }
}

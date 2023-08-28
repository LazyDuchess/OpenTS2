using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class LOADNIL : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            for(var i=A;i<=B;i++)
            {
                context.Code.WriteLine(context.R(i) + " = nil");
            }
        }
    }
}

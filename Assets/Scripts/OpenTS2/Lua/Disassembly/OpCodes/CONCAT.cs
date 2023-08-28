using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class CONCAT : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var strRes = "";
            for(var i=B;i<=C;i++)
            {
                if (i > B)
                    strRes += " .. ";
                strRes += context.R(i);
            }
            context.Code.WriteLine(context.R(A) + " = " + strRes);
        }
    }
}

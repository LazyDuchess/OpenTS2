using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class TAILCALL : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var start = (ushort)(A + 1);
            var end = A + B - 1;
            var callValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    callValues += ", ";
                callValues += context.R((ushort)i);
            }
            var funcReturn = context.ReturnOpCode;
            if (funcReturn == null)
                context.Code.WriteLine("return "+context.R(A)+"("+callValues+")");
            else
            {
                context.Code.WriteLine("ReturnTable = {" + context.R(A) + "(" + callValues + ")}");
                context.Code.WriteGoto("returnLabel");
            }
        }
    }
}

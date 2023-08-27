using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class CALL : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var start = A;
            var end = A + C - 2;
            var retValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    retValues += ", ";
                retValues += context.R((ushort)i);
            }

            start = (ushort)(A + 1);
            end = A + B - 1;
            var callValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    callValues += ", ";
                callValues += context.R((ushort)i);
            }

            if (retValues == "")
                context.Code.WriteLine(context.R(A) + "(" + callValues + ")");
            else
                context.Code.WriteLine(retValues + " = " + context.R(A) + "("+callValues+")");
        }
    }
}

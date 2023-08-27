using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class RETURN : LuaC50.OpCode
    {
        public override void Disassemble(LuaC50.Context context)
        {
            var start = A;
            var end = A + B - 2;
            /*
            if (end < start && context.PC >= context.Function.OpCodes.Count - 1)
                return;*/
            if (end < start)
            {
                context.Code.WriteLine("return true");
                return;
            }    
            var returnValues = "";
            for ( var i = start; i <= end; i++ )
            {
                if (i > start)
                    returnValues += ", ";
                returnValues += context.R(i);
            }
            context.Code.WriteLine("return " + returnValues);
        }
    }
}

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

            context.Code.WriteLine("ReturnTable = {}");

            if (end >= start)
            {
                for (var i = start; i <= end; i++)
                {
                    context.Code.WriteLine($"table.insert(ReturnTable, {context.R(i)})");
                }
            }

            if (context.ReturnOpCode == this)
            {
                context.Code.WriteLabel("returnLabel");
                context.Code.WriteLine("return unpack(ReturnTable)");
            }
            else
                context.Code.WriteGoto("returnLabel");
        }
    }
}

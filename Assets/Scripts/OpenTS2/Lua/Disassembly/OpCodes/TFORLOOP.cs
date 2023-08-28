using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class TFORLOOP : LuaC50.OpCode
    {
        public TFORPREP Begin;
        public override void PreProcess(LuaC50.Context context)
        {
            var jmp = Function.OpCodes[PC + 1] as JMP;
            if (jmp != null)
                jmp.Cancel();
        }

        public override void Disassemble(LuaC50.Context context)
        {
            context.Code.Indentation--;
            context.Code.WriteEnd();
            /*
            var start = A + 2;
            var end = A + 2 + C;
            var retValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    retValues += ", ";
                retValues += context.R((ushort)i);

                // Probably don't need to do this
                //context.Code.WriteLine(context.R((ushort)i) + " = nil");
            }
            context.Code.WriteLine(retValues + " = " + context.R(A) + "("+context.R((ushort)(A+1))+", "+context.R((ushort)(A+2))+")");
            context.Code.WriteLine("if " + context.R((ushort)(A + 2)) + " ~= nil then");
            context.Code.Indentation++;
            context.Code.WriteGoto(context.MakeRelativeJump(2));
            context.Code.Indentation--;
            context.Code.WriteEnd();*/
            //context.Code.WriteLine("for " + loopValues + " in " + context.R((ushort)(A)) + ", " + context.R((ushort)(A + 1)) + " do");
        }
    }
}

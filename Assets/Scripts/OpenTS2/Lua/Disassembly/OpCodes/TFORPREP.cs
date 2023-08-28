using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class TFORPREP : LuaC50.OpCode
    {
        public TFORLOOP End;
        public override void PreProcess(LuaC50.Context context)
        {
            LinkToTFORLOOP();
        }

        public void LinkToTFORLOOP()
        {
            var level = 0;
            var opcodes = Function.OpCodes;
            for (var i = PC+sBx; i < opcodes.Count; i++)
            {
                var opCode = opcodes[i];
                if (opCode is TFORPREP)
                {
                    level++;
                    continue;
                }
                if (opCode is TFORLOOP)
                {
                    if (level == 0)
                    {
                        End = opCode as TFORLOOP;
                        (opCode as TFORLOOP).Begin = this;
                        return;
                    }
                    level--;
                }
            }
        }

        public override void Disassemble(LuaC50.Context context)
        {
            context.Code.WriteLine("for "+ context.R((ushort)(A + 2)) + ", "+ context.R((ushort)(A + 3)) + " in "+ context.R(A) + ", "+ context.R((ushort)(A + 1)) + " do");
            context.Code.Indentation++;
            //context.Code.WriteLine("-- A: " + A);
            //context.Code.WriteLine("-- sbx: " + sBx);
            /*
            context.Code.WriteLine(context.R((ushort)(A + 1)) + " = " + context.R(A)+ " -- tforprep...");
            var jmp = context.MakeRelativeJump(sBx + 1);
            context.Code.WriteGoto(jmp);*/
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class FORLOOP : LuaC50.OpCode
    {
        public override void PreProcess(LuaC50.Context context)
        {
            foreach(var opCode in context.Function.OpCodes)
            {
                var jmp = opCode as JMP;
                if (jmp == null)
                    continue;
                if (jmp.PC == PC + sBx)
                {
                    jmp.Cancel();
                }
            }
            var begin = new LuaC50.BeginFORLOOP(PC + sBx, this);
            context.ForLoops.Add(begin);
        }

        public override void Disassemble(LuaC50.Context context)
        {
            context.Code.Indentation--;
            context.Code.WriteEnd();
        }

        public void DisassembleBegin(LuaC50.Context context, LuaC50.BeginFORLOOP forloop)
        {
            // Here goes the loop begin!
            context.Code.WriteLine("for " + context.R(A) + " = " + context.R(A) + ", " + context.R((ushort)(A + 1)) + ", " + context.R((ushort)(A + 2)) + " do");
            context.Code.Indentation++;
        }
    }
}

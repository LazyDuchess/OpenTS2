using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class CALL : LuaC50.OpCode
    {
        // Some TS2 scripts have invalid arg/return register ranges on CALL instructions. This is the number of registers that will be used when opcodes like this are found.
        static int WorkaroundRegisterAmount = 10;
        public override void Disassemble(LuaC50.Context context)
        {
            var start = A;
            var end = A + C - 2;

            if (start > end+1 && WorkaroundRegisterAmount > 0)
            {
                context.Code.WriteLine("-- HACK: CALL instruction had invalid start and end for return values.");
                context.Code.WriteLine($"-- Return Start: {start}");
                context.Code.WriteLine($"-- Return End: {end}");
                context.Code.WriteLine($"-- Return Diff: {Math.Abs(start - end)}");
                context.Code.WriteLine($"-- A = {A} | B = {B} | C = {C}");
                end = start + WorkaroundRegisterAmount;
            }

            var retValues = "";
            for (var i = start; i <= end; i++)
            {
                if (i > start)
                    retValues += ", ";
                retValues += context.R((ushort)i);
            }

            start = (ushort)(A + 1);
            end = A + B - 1;

            if (start > end+1 && WorkaroundRegisterAmount > 0)
            {
                context.Code.WriteLine("-- HACK: CALL instruction had invalid start and end for arg values.");
                context.Code.WriteLine($"-- Arg Start: {start}");
                context.Code.WriteLine($"-- Arg End: {end}");
                context.Code.WriteLine($"-- Arg Diff: {Math.Abs(start - end)}");
                context.Code.WriteLine($"-- A = {A} | B = {B} | C = {C}");
                end = start + WorkaroundRegisterAmount;
            }

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

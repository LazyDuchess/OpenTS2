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

            var similarReturn = FindSimilarReturnToGoTo();
            if (similarReturn == null)
                context.Code.WriteLine("return "+context.R(A)+"("+callValues+")");
            else
            {
                //context.Code.WriteLine(context.R((ushort)(i - (A - similarReturn.A))) + " = " + context.R(i));
                context.Code.WriteLine(context.R(similarReturn.A) + " = " + context.R(A) + "(" + callValues + ")");
                var jumpLabel = context.MakeAbsoluteJump(similarReturn.PC);
                context.Code.WriteGoto(jumpLabel);
            }
        }

        RETURN FindSimilarReturnToGoTo()
        {
            for (var i = Function.OpCodes.Count - 1; i >= 0; i--)
            {
                var ret = Function.OpCodes[i] as RETURN;
                if (ret == null)
                    continue;
                if (ret.GetReturnValueCount() == 1)
                    return ret;
            }
            return null;
        }
    }
}

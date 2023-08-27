using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class RETURN : LuaC50.OpCode
    {
        public override int GetPCForJumpTarget()
        {
            if (!ValidReturnToDisassemble())
            {
                var aboveMe = PC - 1;
                if (aboveMe >= 0)
                {
                    if (Function.OpCodes[aboveMe] is RETURN)
                        return Function.OpCodes[aboveMe].PC;
                }
            }
            return PC;
        }
        public override void Disassemble(LuaC50.Context context)
        {
            if (!ValidReturnToDisassemble())
            {
                var aboveMe = PC - 1;
                if (aboveMe >= 0)
                {
                    if (Function.OpCodes[aboveMe] is RETURN)
                        return;
                }
            }
            //if (!ValidReturnToDisassemble())
            //    return;
            var similarReturn = FindSimilarReturnToGoTo();
            var start = A;
            var end = A + B - 2;
            if (similarReturn != null)
            {
                if (similarReturn.A != A || similarReturn.B != B)
                {
                    // Remap registers
                    for (var i = start; i <= end; i++)
                    {
                        context.Code.WriteLine(context.R((ushort)(i - (A - similarReturn.A))) + " = " + context.R(i));
                    }
                }
                var jumpLabel = context.MakeAbsoluteJump(similarReturn.PC);
                context.Code.WriteGoto(jumpLabel);
                return;
            }
            
            /*
            if (end < start && context.PC >= context.Function.OpCodes.Count - 1)
                return;*/
            if (end < start)
            {
                context.Code.WriteLine("return");
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

        public int GetReturnValueCount()
        {
            var start = A;
            var end = A + B - 2;
            return (end - start) + 1;
        }

        public bool ValidReturnToDisassemble()
        {
            var start = A;
            var end = A + B - 2;
            if (end < start && PC >= Function.OpCodes.Count - 1)
                return false;
            return true;
        }

        RETURN FindSimilarReturnToGoTo()
        {
            for(var i=Function.OpCodes.Count-1;i>=0;i--)
            {
                var ret = Function.OpCodes[i] as RETURN;
                if (ret == null)
                    continue;
                if (ret == this)
                    return null;
                if (ret.GetReturnValueCount() == GetReturnValueCount())
                    return ret;
                if (ret.A == A && ret.B == B)
                    return ret;
            }
            return null;
        }
    }
}

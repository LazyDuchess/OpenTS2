using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.Disassembly.OpCodes
{
    public class JMP : LuaC50.OpCode
    {
        bool _cancelled = false;
        public override void PreProcess(LuaC50.Context context)
        {
            context.MakeRelativeJump(sBx+1);
        }
        public override void Disassemble(LuaC50.Context context)
        {
            if (_cancelled)
                return;
            var jumpLabel = context.MakeRelativeJump(sBx+1);
            context.Code.WriteGoto(jumpLabel);
        }
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}

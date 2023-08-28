using MoonSharp.Interpreter;
using OpenTS2.SimAntics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.API
{
    public abstract class LuaAPI
    {
        public LuaManager Manager;

        public virtual void PrepareLuaPrimitive(short param0, short param1, short param2)
        {

        }
        public virtual void OnRegister(Script script)
        {

        }
    }
}

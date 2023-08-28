using MoonSharp.Interpreter;
using OpenTS2.Lua.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Engine.Tests
{
    public class LuaTestAPI : LuaAPI
    {
        int GetSimulatorGlobal(int global)
        {
            return 5;
        }
        public override void OnRegister(Script script)
        {
            script.Globals["GetSimulatorGlobal"] = (Func<int, int>)GetSimulatorGlobal;
        }
    }
}

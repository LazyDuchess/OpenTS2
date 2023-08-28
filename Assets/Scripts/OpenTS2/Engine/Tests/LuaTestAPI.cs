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
            switch(global)
            {
                //Year
                case 8:
                    return 2023;
                //Month
                case 7:
                    return 8;
                //Day
                case 1:
                    return 27;
            }
            return 0;
        }
        void Log(string log)
        {
            UnityEngine.Debug.Log(log);
        }
        public override void OnRegister(Script script)
        {
            script.Globals["GetSimulatorGlobal"] = (Func<int, int>)GetSimulatorGlobal;
            script.Globals["UnityLog"] = (Action<string>)Log;
        }
    }
}

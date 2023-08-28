using MoonSharp.Interpreter;
using OpenTS2.SimAntics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.API
{
    public class MainAPI : LuaAPI
    {
        private int[] _primitiveParameters = new int[3];
        private int GetPrimitiveParameter(int param)
        {
            return _primitiveParameters[param];
        }

        private void SetTemp(int temp, int value)
        {
            Manager.Context.Entity.Temps[temp] = (short)value;
        }

        private void SetScriptReturnValue(bool returnValue)
        {
            if (returnValue)
                Manager.ExitCode = VMExitCode.True;
            else
                Manager.ExitCode = VMExitCode.False;
        }

        public override void OnRegister(Script script)
        {
            script.Globals["GetPrimitiveParameter"] = (Func<int, int>)GetPrimitiveParameter;
            script.Globals["SetTemp"] = (Action<int, int>)SetTemp;
            script.Globals["SetScriptReturnValue"] = (Action<bool>)SetScriptReturnValue;
        }

        public override void PrepareLuaPrimitive(short param0, short param1, short param2)
        {
            _primitiveParameters[0] = param0;
            _primitiveParameters[1] = param1;
            _primitiveParameters[2] = param2;
        }
    }
}

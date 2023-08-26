using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using OpenTS2.SimAntics;

namespace OpenTS2.Lua
{
    public class LuaManager
    {
        public VMExitCode ExitCode; 
        public VMContext Context;
        
        private static LuaManager _instance;
        private Script _script;
        private int[] _primitiveParameters = new int[3];

        public static LuaManager Get()
        {
            return _instance;
        }

        public LuaManager()
        {
            _instance = this;
            _script = new Script();
            RegisterGlobals();
        }

        void PrepGlobalsForPrimitive(short param0, short param1, short param2, VMContext ctx)
        {
            _primitiveParameters[0] = param0;
            _primitiveParameters[1] = param1;
            _primitiveParameters[2] = param2;
            Context = ctx;
            ExitCode = VMExitCode.True;
        }

        void ThrowLuaPrimitiveException(ScriptRuntimeException exception)
        {
            throw new SimAnticsException($"Problem executing Lua script:{Environment.NewLine}{exception}", Context.StackFrame);
        }

        public void RunGlobalScript(string lua)
        {
            _script.DoString(lua);
        }

        public VMExitCode RunStringAsPrimitive(string lua, short param0, short param1, short param2, VMContext ctx)
        {
            PrepGlobalsForPrimitive(param0, param1, param2, ctx);
            try
            {
                _script.DoString(lua);
            }
            catch(ScriptRuntimeException e)
            {
                ThrowLuaPrimitiveException(e);
            }
            return ExitCode;
        }

        private int Lua_GetPrimitiveParameter(int param)
        {
            return _primitiveParameters[param];
        }

        private void Lua_SetTemp(int temp, int value)
        {
            Context.Entity.Temps[temp] = (short)value;
        }

        private void Lua_SetScriptReturnValue(bool returnValue)
        {
            if (returnValue)
                ExitCode = VMExitCode.True;
            else
                ExitCode = VMExitCode.False;
        }

        void RegisterGlobals()
        {
            _script.Globals["GetPrimitiveParameter"] = (Func<int,int>)Lua_GetPrimitiveParameter;
            _script.Globals["SetTemp"] = (Action<int, int>)Lua_SetTemp;
            _script.Globals["SetScriptReturnValue"] = (Action<bool>)Lua_SetScriptReturnValue;
        }
    }
}

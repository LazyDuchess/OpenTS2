using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using OpenTS2.Lua.API;
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

        private List<LuaAPI> _apis = new List<LuaAPI>();

        public static LuaManager Get()
        {
            return _instance;
        }

        public LuaManager()
        {
            _instance = this;
            _script = new Script();
            LoadAPIs();
        }

        void PrepGlobalsForPrimitive(short param0, short param1, short param2, VMContext ctx)
        {
            Context = ctx;
            ExitCode = VMExitCode.True;
            foreach (var api in _apis)
                api.PrepareLuaPrimitive(param0, param1, param2);
        }

        void ThrowLuaPrimitiveException(ScriptRuntimeException exception)
        {
            throw new SimAnticsException($"Problem executing Lua script:{Environment.NewLine}{exception}", Context.StackFrame);
        }

        public void RunScript(string lua)
        {
            _script.DoString(lua);
        }

        public VMExitCode RunScriptPrimitive(string lua, short param0, short param1, short param2, VMContext ctx)
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

        void LoadAPIs()
        {
            RegisterAPI(new MainAPI());
        }

        public void RegisterAPI(LuaAPI api)
        {
            _apis.Add(api);
            api.Manager = this;
            api.OnRegister(_script);
        }
    }
}

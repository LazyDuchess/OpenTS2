using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Lua.API;
using OpenTS2.SimAntics;
using UnityEngine;

namespace OpenTS2.Lua
{
    /// <summary>
    /// Manages the global Lua environment.
    /// </summary>
    public class LuaManager
    {
        /// <summary>
        /// Exit code of the current running Lua script called from SimAntics.
        /// </summary>
        public VMExitCode ExitCode;
        /// <summary>
        /// SimAntics context of the current Lua script.
        /// </summary>
        public VMContext Context;
        
        public static LuaManager Instance { get; private set; }
        private Script _script;

        private List<LuaAPI> _apis = new List<LuaAPI>();
        private Dictionary<string, LuaAsset> _objectScriptsByName = new Dictionary<string, LuaAsset>();

        public LuaManager()
        {
            Instance = this;
            _script = new Script();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var element in assemblies)
            {
                UserData.RegisterAssembly(element);
            }
            LoadAPIs();
            Core.OnFinishedLoading += InitializeObjectScripts;
        }

        void PrepGlobalsForPrimitive(short param0, short param1, short param2, VMContext ctx)
        {
            Context = ctx;
            ExitCode = VMExitCode.True;
            foreach (var api in _apis)
                api.PrepareLuaPrimitive(param0, param1, param2);
        }

        void ThrowLuaPrimitiveException(ScriptRuntimeException exception, string name)
        {
            throw new SimAnticsException($"Problem executing Lua script {name}:{Environment.NewLine}{exception.DecoratedMessage}", Context.StackFrame);
        }

        /// <summary>
        /// Loads and indexes all object scripts that can be found in the latest installed product, and runs all global scripts.
        /// </summary>
        public void InitializeObjectScripts()
        {
            var objectScripts = Filesystem.GetLatestFilePath("Res/ObjectScripts/ObjectScripts.package");
            if (objectScripts == null)
            {
                Debug.Log("LuaManager: No object scripts in current product.");
                return;
            }
            var objectScriptsFile = new DBPFFile(objectScripts);

            foreach(var entry in objectScriptsFile.Entries)
            {
                if (entry.TGI.TypeID != TypeIDs.LUA_GLOBAL && entry.TGI.TypeID != TypeIDs.LUA_LOCAL)
                    continue;
                try
                {
                    var luaAsset = entry.GetAsset<LuaAsset>();
                    try
                    {
                        switch (entry.TGI.TypeID)
                        {
                            case TypeIDs.LUA_GLOBAL:
                                RunScript(luaAsset.Source);
                                break;
                            case TypeIDs.LUA_LOCAL:
                                _objectScriptsByName[luaAsset.FileName] = luaAsset;
                                break;
                        }
                    }
                    catch(InterpreterException e)
                    {
                        Debug.LogError($"LuaManager: Failed to run object script {luaAsset.FileName}:{Environment.NewLine}{e.DecoratedMessage}");
                    }

                }
                catch(Exception e)
                {
                    Debug.LogError($"LuaManager: Unable to load object script {entry.GlobalTGI}:{Environment.NewLine}{e}");
                }
            }
        }

        /// <summary>
        /// Retrieves a Lua script from the ObjectScripts package.
        /// </summary>
        /// <param name="name">Name of the script.</param>
        /// <returns>Lua source code.</returns>
        public string GetObjectScript(string name)
        {
            if (_objectScriptsByName.TryGetValue(name, out LuaAsset script))
                return script.Source;
            return null;
        }

        /// <summary>
        /// Runs a Lua script in a global context.
        /// </summary>
        public void RunScript(string lua)
        {
            _script.DoString(lua);
        }

        /// <summary>
        /// Runs a Lua script as part of a SimAntics call.
        /// </summary>
        /// <param name="lua">Lua source code to run.</param>
        /// <param name="param0">Parameter 0</param>
        /// <param name="param1">Parameter 1</param>
        /// <param name="param2">Parameter 2</param>
        /// <param name="ctx">SimAntics Context</param>
        /// <returns>The exit code for this primitive.</returns>
        public VMExitCode RunScriptPrimitive(string name, string lua, short param0, short param1, short param2, VMContext ctx)
        {
            PrepGlobalsForPrimitive(param0, param1, param2, ctx);
            try
            {
                _script.DoString(lua);
            }
            catch(ScriptRuntimeException e)
            {
                ThrowLuaPrimitiveException(e, name);
            }
            return ExitCode;
        }

        void LoadAPIs()
        {
            RegisterAPI(new MainAPI());
            RegisterAPI(new ObjectAPI());
        }

        public void RegisterAPI(LuaAPI api)
        {
            _apis.Add(api);
            api.Manager = this;
            api.OnRegister(_script);
        }
    }
}

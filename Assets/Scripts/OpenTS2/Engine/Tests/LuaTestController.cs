using MoonSharp.Interpreter;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class LuaTestController : MonoBehaviour
    {
        void Start()
        {
            LuaManager.Get().RegisterAPI(new LuaTestAPI());

            var objectScripts = Filesystem.GetLatestFilePath("Res/ObjectScripts/ObjectScripts.package");
            Debug.Log($"Reading scripts from {objectScripts}");
            var dbpf = new DBPFFile(objectScripts);
            foreach (var entry in dbpf.Entries)
            {
                if (entry.GlobalTGI.TypeID != TypeIDs.LUA_GLOBAL)
                    continue;
                var luaAsset = entry.GetAsset<LuaAsset>();
                File.WriteAllText($"TestFiles/{luaAsset.FileName}.lua", luaAsset.Source);
                try
                {
                    LuaManager.Get().RunScript(luaAsset.Source);
                }
                catch(Exception e)
                {
                    var msg = e.ToString();
                    if (e is InterpreterException)
                        msg = (e as InterpreterException).DecoratedMessage;
                    Debug.LogError($"Problem running {luaAsset.FileName}:{msg}");
                }
            }

            try
            {
                LuaManager.Get().RunScript(@"local time = nTime.Now()
                                            UnityLog('Lua Year is ' .. time.mYears)
                                            UnityLog('Lua Month is ' .. time.mMonths)
                                            UnityLog('Lua Day is ' .. time.mDays)");
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                if (e is InterpreterException)
                    msg = (e as InterpreterException).DecoratedMessage;
                Debug.LogError($"Problem running:{msg}");
            }
}
    }
}
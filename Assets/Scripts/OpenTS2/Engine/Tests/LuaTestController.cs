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
        public string ScriptToRun = "GameMessages";
        // Start is called before the first frame update
        void Start()
        {
            var objectScripts = Filesystem.GetLatestFilePath("Res/ObjectScripts/ObjectScripts.package");
            var dbpf = new DBPFFile(objectScripts);
            LuaAsset luaAsset = null;
            foreach (var entry in dbpf.Entries)
            {
                
                try
                {
                    var thisLuaAsset = entry.GetAsset<LuaAsset>();
                    if (thisLuaAsset.FileName == ScriptToRun)
                    {
                        Debug.Log(thisLuaAsset.FileName);
                        try
                        {
                            luaAsset = thisLuaAsset;
                            File.WriteAllText($"TestFiles/{ScriptToRun}.lua", thisLuaAsset.Source);
                        }
                        catch(Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.Log(e);
                }
                
            }
            try
            {
                LuaManager.Get().RunGlobalScript(luaAsset.Source);
            }
            catch(InterpreterException e)
            {
                Debug.LogError(e.DecoratedMessage);
            }
            /*
            LuaManager.Get().RunGlobalScript(@"R_0 = {}
Math = R_0
R_0 = Math

function R_1(R_0, R_1, R_2, R_3)
		if (R_3 ~= 0) then 
	R_3 = R_3

			if (R_0 == R_2) then
			R_4 = true
			return R_4
		end
			if (R_1 == R_2) then
			R_4 = true
			return R_4
		end
	end
		if (R_2 < R_0) then
			if (R_2 < R_1) then
			R_4 = false
			return R_4
		end
		R_4 = true
		return R_4
	else
			if (R_1 < R_2) then
			R_4 = false
			return R_4
		end
		R_4 = true
		return R_4
	end
end


R_0["+"\"InRange\""+ @"] = R_1
");
            var luaMgr = LuaManager.Get();
            UnityEngine.Debug.Log(luaMgr._script.Call(luaMgr._script.Globals["R_1"], 2, 4, 2, 1));}
        */
        }
    }
}
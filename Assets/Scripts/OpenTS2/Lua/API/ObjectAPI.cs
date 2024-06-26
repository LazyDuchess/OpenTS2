using MoonSharp.Interpreter;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Lua.API
{
    [MoonSharpUserData]
    class GlobalObjManager
    {
        public bool isValidObjectGUID(uint guid)
        {
            if (ObjectManager.GetObjectByGUID(guid) != null)
                return true;
            return false;
        }
    }
    public class ObjectAPI : LuaAPI
    {
        void SetObjectDefinitionField(uint guid, int field, ushort value)
        {
            var obj = ObjectManager.GetObjectByGUID(guid);
            if (obj == null)
                throw new ScriptRuntimeException($"Object with GUID {guid} does not exist.");
            obj.Fields[field] = value;
        }
        public override void OnRegister(Script script)
        {
            script.Globals["GlobalObjManager"] = new GlobalObjManager();
            script.Globals["SetObjectDefinitionField"] = (Action<uint, int, ushort>)SetObjectDefinitionField;
        }
    }
}

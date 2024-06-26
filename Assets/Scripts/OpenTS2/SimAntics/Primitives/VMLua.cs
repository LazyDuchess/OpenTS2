using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Lua;

namespace OpenTS2.SimAntics.Primitives
{
    public class VMLua : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var stringSetID = ctx.Node.GetUInt16Operand(0);
            var stringIndex = ctx.Node.GetUInt16Operand(2) - 1;

            var flags = ctx.Node.GetOperand(4);

            var useFile = (flags & 1) == 1;
            var passParams = (flags & 8) == 8;

            var privateScope = (flags & 2) == 2;
            var semiGlobalScope = (flags & 4) == 4;

            var stringGroupID = GroupIDs.Global;

            if (privateScope)
                stringGroupID = ctx.Entity.PrivateGroupID;

            if (semiGlobalScope)
                stringGroupID = ctx.Entity.SemiGlobalGroupID;

            short param0 = 0;
            short param1 = 0;
            short param2 = 0;

            if (passParams)
            {
                var param0DataSource = (VMDataSource)ctx.Node.GetOperand(6);
                var param1DataSource = (VMDataSource)ctx.Node.GetOperand(9);
                var param2DataSource = (VMDataSource)ctx.Node.GetOperand(12);

                var param0DataValue = ctx.Node.GetInt16Operand(7);
                var param1DataValue = ctx.Node.GetInt16Operand(10);
                var param2DataValue = ctx.Node.GetInt16Operand(13);

                param0 = ctx.GetData(param0DataSource, param0DataValue);
                param1 = ctx.GetData(param1DataSource, param1DataValue);
                param2 = ctx.GetData(param2DataSource, param2DataValue);
            }

            var luaStringSet = ContentManager.Instance.GetAsset<StringSetAsset>(new ResourceKey(stringSetID, stringGroupID, TypeIDs.STR));

            var scriptName = luaStringSet.StringData.GetString(stringIndex, Languages.USEnglish);
            var scriptDesc = luaStringSet.StringData.GetDescription(stringIndex, Languages.USEnglish);

            var scriptSource = scriptDesc;

            var luaManager = LuaManager.Instance;

            if (useFile)
            {
                scriptSource = luaManager.GetObjectScript(scriptName);
                if (scriptSource == null)
                    throw new SimAnticsException($"Can't find Lua object script named {scriptName}", ctx.StackFrame);
            }

            var luaResult = luaManager.RunScriptPrimitive(scriptName, scriptSource, param0, param1, param2, ctx);

            return new VMReturnValue(luaResult);
        }
    }
}

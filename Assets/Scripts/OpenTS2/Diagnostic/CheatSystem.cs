using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Diagnostic
{
    public static class CheatSystem
    {
        private static Dictionary<string, Cheat> s_cheatsByName = new Dictionary<string, Cheat>();
        private static Dictionary<string, ConsoleProperty> s_propertiesByName = new Dictionary<string, ConsoleProperty>();
        public static void Initialize()
        {
            RegisterCheat<BoolPropCheat>();
            Assemblies.AssemblyHelper.AssemblyProcesses += RegisterPropsForType;
        }

        private static void RegisterPropsForType(Type type, Assembly assembly)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach(var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(ConsoleProperty)))
                {
                    var prop = field.GetValue(null) as ConsoleProperty;
                    RegisterProperty(prop, prop.Name);
                }
            }
        }

        public static void RegisterCheat<T>() where T : Cheat
        {
            var cheat = Activator.CreateInstance(typeof(T)) as Cheat;
            s_cheatsByName[cheat.Name.Trim().ToLowerInvariant()] = cheat;
        }

        public static void RegisterProperty(ConsoleProperty property, string name)
        {
            s_propertiesByName[name.Trim().ToLowerInvariant()] = property;
        }

        public static void Execute(string command)
        {
            var cheatArgs = new CheatArguments(command);
            var cmd = cheatArgs.GetString(0).Trim().ToLowerInvariant();
            var cheat = s_cheatsByName[cmd];
            cheat.Execute(cheatArgs);
        }

        public static bool ParseBoolean(string arg)
        {
            var parsedArg = arg.Trim().ToLowerInvariant();
            if (parsedArg == "true")
                return true;
            if (parsedArg == "on")
                return true;
            var parsedInt = int.TryParse(parsedArg, out int asInt);
            if (parsedInt)
            {
                if (asInt > 0)
                    return true;
            }
            return false;
        }

        public static ConsoleProperty GetProperty(string name)
        {
            var nameParsed = name.Trim().ToLowerInvariant();
            return s_propertiesByName[nameParsed];
        }
    }
}

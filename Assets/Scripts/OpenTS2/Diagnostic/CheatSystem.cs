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
        public static Dictionary<string, Cheat> CheatsByName = new Dictionary<string, Cheat>();
        public static Dictionary<string, IConsoleProperty> PropertiesByName = new Dictionary<string, IConsoleProperty>();
        public static void Initialize()
        {
            ConsolePropertyCheats.RegisterAllCheats();
            RegisterCheat<HelpCheat>();
            RegisterCheat<PropsCheat>();
            RegisterCheat<ClearCheat>();
            RegisterCheat<GoToShellCheat>();
            Assemblies.AssemblyHelper.AssemblyProcesses += RegisterPropsForType;
        }

        private static void RegisterPropsForType(Type type, Assembly assembly)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var props = type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<ConsolePropertyAttribute>();
                if (attr != null)
                {
                    var name = attr.Name;
                    if (string.IsNullOrEmpty(name))
                        name = field.Name;
                    var fieldProp = new FieldConsoleProperty(field);
                    RegisterProperty(fieldProp, name);
                }
            }
            foreach (var prop in fields)
            {
                var attr = prop.GetCustomAttribute<ConsolePropertyAttribute>();
                if (attr != null)
                {
                    var name = attr.Name;
                    if (string.IsNullOrEmpty(name))
                        name = prop.Name;
                    var fieldProp = new FieldConsoleProperty(prop);
                    RegisterProperty(fieldProp, name);
                }
            }
        }

        public static void RegisterCheat<T>() where T : Cheat
        {
            var cheat = Activator.CreateInstance(typeof(T)) as Cheat;
            CheatsByName[cheat.Name.Trim().ToLowerInvariant()] = cheat;
        }

        public static void RegisterProperty(IConsoleProperty property, string name)
        {
            PropertiesByName[name.Trim().ToLowerInvariant()] = property;
        }

        public static void Execute(string command, IConsoleOutput output = null)
        {
            var cheatArgs = new CheatArguments(command);
            var cmd = cheatArgs.GetString(0).Trim().ToLowerInvariant();
            var cheat = CheatsByName[cmd];
            cheat.Execute(cheatArgs, output);
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

        public static IConsoleProperty GetProperty(string name)
        {
            var nameParsed = name.Trim().ToLowerInvariant();
            return PropertiesByName[nameParsed];
        }
    }
}

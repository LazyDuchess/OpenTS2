using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Diagnostic
{
    public static class ConsolePropertyCheats
    {
        public static void RegisterAllCheats()
        {
            CheatSystem.RegisterCheat<BoolPropCheat>();
            CheatSystem.RegisterCheat<IntPropCheat>();
            CheatSystem.RegisterCheat<UIntPropCheat>();
            CheatSystem.RegisterCheat<FloatPropCheat>();
            CheatSystem.RegisterCheat<StringPropCheat>();
        }
    }
    public class BoolPropCheat : Cheat
    {
        public override string Name => "boolprop";

        public override void Execute(CheatArguments arguments, IConsoleOutput output)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (property.GetValueType() != typeof(bool))
                throw new ArgumentException("Passed property is not a boolean.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
    public class IntPropCheat : Cheat
    {
        public override string Name => "intprop";

        public override void Execute(CheatArguments arguments, IConsoleOutput output)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (property.GetValueType() != typeof(int))
                throw new ArgumentException("Passed property is not an int.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
    public class UIntPropCheat : Cheat
    {
        public override string Name => "uintprop";

        public override void Execute(CheatArguments arguments, IConsoleOutput output)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (property.GetValueType() != typeof(uint))
                throw new ArgumentException("Passed property is not an uint.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
    public class FloatPropCheat : Cheat
    {
        public override string Name => "floatprop";

        public override void Execute(CheatArguments arguments, IConsoleOutput output)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (property.GetValueType() != typeof(float))
                throw new ArgumentException("Passed property is not a float.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
    public class StringPropCheat : Cheat
    {
        public override string Name => "stringprop";

        public override void Execute(CheatArguments arguments, IConsoleOutput output)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (property.GetValueType() != typeof(string))
                throw new ArgumentException("Passed property is not a string.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
}

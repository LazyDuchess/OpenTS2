using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Diagnostic
{
    public class BoolPropCheat : Cheat
    {
        public override string Name => "boolprop";

        public override void Execute(CheatArguments arguments)
        {
            if (arguments.Count != 3)
                throw new ArgumentException("Must pass 3 arguments only.");
            var property = CheatSystem.GetProperty(arguments.GetString(1));
            if (!(property is BooleanProperty))
                throw new ArgumentException("Passed property is not a BooleanProperty.");
            property.SetStringValue(arguments.GetString(2));
        }
    }
}

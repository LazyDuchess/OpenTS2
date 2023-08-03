using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    /// <summary>
    /// On static variables, allows editing via the cheat console (F3) using the boolprop/intprop/uintprop/stringprop/floatprop cheats.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ConsolePropertyAttribute : Attribute
    {
        public string Name => _name;
        private readonly string _name = "";
        public ConsolePropertyAttribute(string name)
        {
            _name = name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Engine
{
    /// <summary>
    /// On static variables, allows editing via the cheat console (F3) using the boolprop/intprop/uintprop/stringprop/floatprop cheats.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class GamePropertyAttribute : Attribute
    {
        public string Name { get; private set; }
        public bool User { get; private set; }

        public GamePropertyAttribute(bool userProp)
        {
            User = userProp;
        }

        public GamePropertyAttribute(string name, bool userProp)
        {
            Name = name;
            User = userProp;
        }
    }
}

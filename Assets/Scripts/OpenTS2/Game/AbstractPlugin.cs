using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Game
{
    /// <summary>
    /// Base plugin class. Constructor called on load.
    /// </summary>
    public abstract class AbstractPlugin
    {
        public Assembly Assembly;
    }
}

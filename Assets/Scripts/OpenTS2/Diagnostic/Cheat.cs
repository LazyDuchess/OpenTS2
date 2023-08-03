using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    /// <summary>
    /// Cheat that can be executed via the cheat console (F3)
    /// </summary>
    public abstract class Cheat
    {
        public abstract string Name { get; }
        public virtual string Description => "";
        public abstract void Execute(CheatArguments arguments, IConsoleOutput output = null);
    }
}

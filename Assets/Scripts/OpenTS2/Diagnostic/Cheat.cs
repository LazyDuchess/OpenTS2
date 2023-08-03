using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public abstract class Cheat
    {
        public abstract string Name { get; }
        public virtual string Description => "";
        public abstract void Execute(CheatArguments arguments);
    }
}

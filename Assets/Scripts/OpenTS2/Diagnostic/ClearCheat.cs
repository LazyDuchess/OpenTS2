using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class ClearCheat : Cheat
    {
        public override string Name => "clear";
        public override string Description => "Clears the cheat console";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            output?.Clear();
        }
    }
}

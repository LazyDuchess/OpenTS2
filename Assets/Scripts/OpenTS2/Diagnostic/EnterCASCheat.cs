using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class EnterCASCheat : Cheat
    {
        public override string Name => "enterCAS";
        public override string Description => "Transitions to CAS.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            CASManager.Instance.EnterCAS();
        }
    }
}

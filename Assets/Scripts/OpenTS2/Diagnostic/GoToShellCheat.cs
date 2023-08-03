using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class GoToShellCheat : Cheat
    {
        public override string Name => "goToShell";
        public override string Description => "Transitions back to the shell mode.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            NeighborhoodManager.LeaveNeighborhood();
        }
    }
}

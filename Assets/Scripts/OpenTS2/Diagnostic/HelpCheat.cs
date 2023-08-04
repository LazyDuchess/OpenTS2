using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class HelpCheat : Cheat
    {
        public override string Name => "help";

        public override string Description => "Displays all available console cheats.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            var wildcard = "";
            if (arguments.Count > 1)
                wildcard = arguments.GetString(1).Trim().ToLowerInvariant();
            var allCheats = CheatSystem.CheatsByName;
            foreach(var cheat in allCheats)
            {
                if (!string.IsNullOrEmpty(wildcard))
                {
                    if (!cheat.Key.Contains(wildcard))
                        continue;
                }
                var str = cheat.Value.Name;
                var desc = cheat.Value.Description;
                if (!string.IsNullOrEmpty(desc))
                {
                    str += $" - {desc}";
                }
                output?.Log(str);
            }
        }
    }
}

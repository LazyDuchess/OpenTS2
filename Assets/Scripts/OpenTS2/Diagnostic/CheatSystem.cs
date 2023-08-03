using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public static class CheatSystem
    {
        private static Dictionary<string, Cheat> s_cheatsByName = new Dictionary<string, Cheat>();
        public static void Initialize()
        {

        }

        public static void RegisterCheat(Cheat cheat)
        {
            s_cheatsByName[cheat.Name] = cheat;
        }

        public static void Execute(string command)
        {
            var cheatArgs = new CheatArguments(command);
            var cmd = cheatArgs.GetString(0).Trim().ToLowerInvariant();
            var cheat = s_cheatsByName[cmd];
            cheat.Execute(cheatArgs);
        }
    }
}

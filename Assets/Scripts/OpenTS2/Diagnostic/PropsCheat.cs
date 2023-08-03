using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class PropsCheat : Cheat
    {
        public override string Name => "props";

        public override string Description => "Displays all available console properties.";

        public override void Execute(CheatArguments arguments, IConsoleOutput output = null)
        {
            var wildcard = "";
            if (arguments.Count > 1)
                wildcard = arguments.GetString(1).Trim().ToLowerInvariant();
            var allProps = CheatSystem.PropertiesByName;
            foreach (var prop in allProps)
            {
                if (!string.IsNullOrEmpty(wildcard))
                {
                    if (!prop.Key.Contains(wildcard))
                        continue;
                }
                var str = $"({prop.Value.GetValueType()}) {prop.Key}: {prop.Value.GetStringValue()}";
                output?.Log(str);
            }
        }
    }
}

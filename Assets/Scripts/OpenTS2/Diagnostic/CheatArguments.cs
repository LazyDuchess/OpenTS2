using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class CheatArguments
    {
        private List<string> _args;
        public int Count => _args.Count;
        public bool GetBool(int index)
        {
            var arg = _args[index];
            var parsedArg = arg.Trim().ToLowerInvariant();
            if (parsedArg == "true")
                return true;
            if (parsedArg == "on")
                return true;
            var parsedInt = int.TryParse(parsedArg, out int asInt);
            if (parsedInt)
            {
                if (asInt > 0)
                    return true;
            }
            return false;
        }
        public string GetString(int index)
        {
            return _args[index];
        }

        public int GetInt(int index)
        {
            return int.Parse(_args[index]);
        }

        public CheatArguments(string command)
        {
            _args = new List<string>();
            var inString = false;
            var stringSoFar = "";
            for(var i=0;i<command.Length;i++)
            {
                if (command[i] == '"')
                {
                    inString = !inString;
                    continue;
                }
                if (command[i] == ' ' && !inString)
                {
                    _args.Add(stringSoFar);
                    continue;
                }
                stringSoFar += command[i];
            }
            if (!string.IsNullOrEmpty(stringSoFar))
                _args.Add(stringSoFar);
        }
    }
}

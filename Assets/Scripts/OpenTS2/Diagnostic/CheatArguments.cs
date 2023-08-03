using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    /// <summary>
    /// Arguments passed to the cheat console.
    /// </summary>
    public class CheatArguments
    {
        private List<string> _args;
        public int Count => _args.Count;
        public bool GetBool(int index)
        {
            var arg = _args[index];
            return CheatSystem.ParseBoolean(arg);
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
                    if (inString)
                    {
                        _args.Add(stringSoFar);
                        stringSoFar = "";
                    }
                    inString = !inString;
                    continue;
                }
                if (command[i] == ' ' && !inString)
                {
                    if (stringSoFar != "")
                    {
                        _args.Add(stringSoFar);
                        stringSoFar = "";
                    }
                    continue;
                }
                stringSoFar += command[i];
            }
            if (!string.IsNullOrEmpty(stringSoFar))
                _args.Add(stringSoFar);
        }
    }
}

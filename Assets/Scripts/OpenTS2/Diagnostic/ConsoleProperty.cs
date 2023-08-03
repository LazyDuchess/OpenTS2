using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public abstract class ConsoleProperty
    {
        public string Name => _name;
        private readonly string _name = "";
        public ConsoleProperty(string name)
        {
            _name = name;
        }
        public abstract void SetStringValue(string value);
        public abstract string GetStringValue();
    }
}

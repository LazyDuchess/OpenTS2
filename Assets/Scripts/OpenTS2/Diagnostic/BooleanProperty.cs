using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class BooleanProperty : ConsoleProperty
    {
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        private bool _value;
        public BooleanProperty(string name, bool defaultValue) : base(name)
        {
            _value = defaultValue;
        }
        public override string GetStringValue()
        {
            return Value ? "true" : "false";
        }

        public override void SetStringValue(string value)
        {
            Value = CheatSystem.ParseBoolean(value);
        }
    }
}

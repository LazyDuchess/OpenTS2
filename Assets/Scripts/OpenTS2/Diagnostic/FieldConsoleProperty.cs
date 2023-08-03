using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class FieldConsoleProperty : IConsoleProperty
    {
        private readonly FieldInfo _field;
        public FieldConsoleProperty(FieldInfo field)
        {
            _field = field;
        }
        public string GetStringValue()
        {
            return _field.GetValue(null).ToString();
        }

        public Type GetValueType()
        {
            return _field.FieldType;
        }

        public void SetStringValue(string value)
        {
            if (_field.FieldType == typeof(bool))
            {
                var boolValue = CheatSystem.ParseBoolean(value);
                _field.SetValue(null, boolValue);
                return;
            }
            if (_field.FieldType == typeof(int))
            {
                _field.SetValue(null, int.Parse(value));
                return;
            }
            if (_field.FieldType == typeof(uint))
            {
                _field.SetValue(null, uint.Parse(value));
                return;
            }
            if (_field.FieldType == typeof(float))
            {
                _field.SetValue(null, float.Parse(value));
                return;
            }
            _field.SetValue(null, value);
        }
    }
}

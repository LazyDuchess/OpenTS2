using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public class PropertyConsoleProperty : IConsoleProperty
    {
        private readonly PropertyInfo _property;
        public PropertyConsoleProperty(PropertyInfo property)
        {
            _property = property;
        }
        public string GetStringValue()
        {
            return _property.GetValue(null).ToString();
        }

        public Type GetValueType()
        {
            return _property.PropertyType;
        }

        public void SetStringValue(string value)
        {
            if (_property.PropertyType == typeof(bool))
            {
                var boolValue = CheatSystem.ParseBoolean(value);
                _property.SetValue(null, boolValue);
                return;
            }
            if (_property.PropertyType == typeof(int))
            {
                _property.SetValue(null, int.Parse(value));
                return;
            }
            if (_property.PropertyType == typeof(uint))
            {
                _property.SetValue(null, uint.Parse(value));
                return;
            }
            if (_property.PropertyType == typeof(float))
            {
                _property.SetValue(null, float.Parse(value));
                return;
            }
            _property.SetValue(null, value);
        }
    }
}

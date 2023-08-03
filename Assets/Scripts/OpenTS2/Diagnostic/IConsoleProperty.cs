using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Diagnostic
{
    public interface IConsoleProperty
    {
        public void SetStringValue(string value);
        public string GetStringValue();
        public Type GetValueType();
    }
}

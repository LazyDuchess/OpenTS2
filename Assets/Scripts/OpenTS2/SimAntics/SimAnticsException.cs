using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class SimAnticsException : Exception
    {
        public VMStackFrame StackFrame;
        public SimAnticsException(string message, VMStackFrame stackFrame) : base(message)
        {
            StackFrame = stackFrame;
        }
    }
}

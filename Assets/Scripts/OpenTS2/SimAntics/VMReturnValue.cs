using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public struct VMReturnValue
    {
        public VMExitCode Code;
        public VMContinueHandler ContinueHandler;
        public static VMReturnValue ReturnTrue = new VMReturnValue(VMExitCode.True);
        public static VMReturnValue ReturnFalse = new VMReturnValue(VMExitCode.False);
        
        public VMReturnValue(VMExitCode exitCode)
        {
            Code = exitCode;
            ContinueHandler = null;
        }
        
        public VMReturnValue(VMContinueHandler continueHandler)
        {
            Code = VMExitCode.Continue;
            ContinueHandler = continueHandler;
        }
    }
}

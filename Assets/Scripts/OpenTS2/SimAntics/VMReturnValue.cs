using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public struct VMReturnValue
    {
        public bool Halt;
        public VMExitCode Code;
        public VMContinueHandler ContinueHandler;
        public static VMReturnValue ReturnTrue = new VMReturnValue(VMExitCode.True);
        public static VMReturnValue ReturnFalse = new VMReturnValue(VMExitCode.False);
        /// <summary>
        /// Stops execution of further nodes in the script, returns true.
        /// </summary>
        public static VMReturnValue HaltAndReturnTrue = new VMReturnValue(VMExitCode.True, true);
        /// <summary>
        /// Stops execution of further nodes in the script, returns false.
        /// </summary>
        public static VMReturnValue HaltAndReturnFalse = new VMReturnValue(VMExitCode.False, true);
        
        public VMReturnValue(VMExitCode exitCode, bool halt = false)
        {
            Halt = halt;
            Code = exitCode;
            ContinueHandler = null;
        }
        
        public VMReturnValue(VMContinueHandler continueHandler)
        {
            Halt = false;
            Code = VMExitCode.Continue;
            ContinueHandler = continueHandler;
        }
    }
}

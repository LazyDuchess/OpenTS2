using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMContinueHandler
    {
        public VMExitCode ExitCode = VMExitCode.Continue;
        public VMExitCode Tick()
        {
            Handle();
            return ExitCode;
        }
        protected virtual void Handle()
        {

        }
    }
}

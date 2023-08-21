using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMReturnValue
    {
        public enum ExitCode : byte
        {
            GoToTrue,
            GoToFalse,
            Continue
        }
        public ExitCode Code;
        public Func<ExitCode> ContinueCallback;
        public static VMReturnValue ReturnTrue = new VMReturnValue { Code = ExitCode.GoToTrue };
        public static VMReturnValue ReturnFalse = new VMReturnValue { Code = ExitCode.GoToFalse };
        // TODO: would probably be a good idea to implement a scheduler for this.
        public static VMReturnValue Sleep(VM vm, uint ticks)
        {
            var returnValue = new VMReturnValue();
            var targetTick = vm.CurrentTick + ticks;
            returnValue.Code = ExitCode.Continue;
            returnValue.ContinueCallback = () => {
                {
                    if (vm.CurrentTick == targetTick)
                        return ExitCode.GoToTrue;
                    return ExitCode.Continue;
                }
            };
            return returnValue;
        }
    }
}

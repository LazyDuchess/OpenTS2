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
            True,
            False,
            Continue
        }
        public ExitCode Code;
        /// <summary>
        /// This callback will be invoked every tick if the ExitCode is Continue until it returns something other than Continue.
        /// </summary>
        public Func<ExitCode> ContinueCallback = null;
        public static VMReturnValue ReturnTrue = new VMReturnValue { Code = ExitCode.True };
        public static VMReturnValue ReturnFalse = new VMReturnValue { Code = ExitCode.False };
        // TODO: would probably be a good idea to implement a scheduler for this.
        public static VMReturnValue Sleep(VM vm, uint ticks)
        {
            var returnValue = new VMReturnValue();
            var targetTick = vm.CurrentTick + ticks;
            returnValue.Code = ExitCode.Continue;
            returnValue.ContinueCallback = () => {
                {
                    if (vm.CurrentTick >= targetTick)
                        return ExitCode.True;
                    return ExitCode.Continue;
                }
            };
            return returnValue;
        }
    }
}

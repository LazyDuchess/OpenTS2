using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public abstract class VMPrimitive
    {
        public abstract ReturnValue Execute();

        public enum ExitCode : byte
        {
            GoToTrue,
            GoToFalse
        }

        public struct ReturnValue
        {
            private ExitCode Code;
            public int Ticks;
            
            public static ReturnValue GoToTrue = new ReturnValue { Code = ExitCode.GoToTrue, Ticks = 0 };
            public static ReturnValue GoToFalse = new ReturnValue { Code = ExitCode.GoToFalse, Ticks = 0 };
            public static ReturnValue ContinueOnFutureTick( ExitCode exitCode, int ticksToSleep )
            {
                return new ReturnValue
                {
                    Code = exitCode,
                    Ticks = ticksToSleep
                };
            }
        }
    }
}

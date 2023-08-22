using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMSleepContinueHandler : VMContinueHandler
    {
        public uint TargetTick = 0;
        VMStack _stack;

        public VMSleepContinueHandler(VMStack stack, uint ticks)
        {
            _stack = stack;
            var vm = _stack.Entity.VM;
            TargetTick = vm.CurrentTick + ticks;
        }

        public override VMExitCode Tick()
        {
            if (_stack.Interrupt)
            {
                // Handled!
                _stack.Interrupt = false;
                return VMExitCode.True;
            }
            if (_stack.Entity.VM.CurrentTick >= TargetTick)
                return VMExitCode.True;
            return VMExitCode.Continue;
        }
    }
}

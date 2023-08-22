using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMSleepContinueHandler : VMContinueHandler
    {
        public bool AllowPush = false;
        public bool AllowNotify = false;
        public uint TargetTick = 0;
        bool _wokenUp = false;
        VM _vm;

        public VMSleepContinueHandler(VM vm, uint ticks, bool allowNotify = false, bool allowPush = false)
        {
            _vm = vm;
            TargetTick = vm.CurrentTick + ticks;
            AllowNotify = allowNotify;
            AllowPush = allowPush;
        }

        public override VMExitCode Tick()
        {
            if (_wokenUp)
                return VMExitCode.True;
            if (_vm.CurrentTick >= TargetTick)
                return VMExitCode.True;
            return VMExitCode.Continue;
        }

        public void WakeUpPush()
        {
            if (AllowPush)
                _wokenUp = true;
        }

        public void WakeUpNotify()
        {
            if (AllowNotify)
                _wokenUp = true;
        }
    }
}

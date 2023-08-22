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
        VM _vm;

        public VMSleepContinueHandler(VM vm, uint ticks, bool allowNotify = false, bool allowPush = false)
        {
            _vm = vm;
            TargetTick = vm.CurrentTick + ticks;
            AllowNotify = allowNotify;
            AllowPush = allowPush;
        }

        protected override void Handle()
        {
            if (_vm.CurrentTick >= TargetTick)
                ExitCode = VMExitCode.True;
        }
    }
}

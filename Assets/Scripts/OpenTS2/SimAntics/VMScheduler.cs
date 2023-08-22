using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMScheduler
    {
        private VMScheduledEvents _preTickEvents = new VMScheduledEvents();
        private VMScheduledEvents _postTickEvents = new VMScheduledEvents();

        public void ScheduleOnBeginTick(Action func, bool onlyOnce = true, uint targetTick = 0)
        {
            _preTickEvents.AddEvent(new VMEvent(func, onlyOnce, targetTick));
        }

        public void ScheduleOnEndTick(Action func, bool onlyOnce = true, uint targetTick = 0)
        {
            _postTickEvents.AddEvent(new VMEvent(func, onlyOnce, targetTick));
        }

        public void ScheduleInterrupt(VMStack stack)
        {
            ScheduleOnBeginTick(() =>
            {
                stack.Interrupt = true;
            });
        }

        public void OnBeginTick(VM vm)
        {
            _preTickEvents.Run(vm);
        }

        public void OnEndTick(VM vm)
        {
            _postTickEvents.Run(vm);
        }

        public class VMEvent
        {
            public Action Event;
            public bool OnlyOnce = true;
            public uint TargetTick = 0;

            public VMEvent(Action func, bool onlyOnce = true, uint targetTick = 0)
            {
                Event = func;
                OnlyOnce = onlyOnce;
                TargetTick = targetTick;
            }
        }

        public class VMScheduledEvents
        {
            private List<VMEvent> _events = new List<VMEvent>();
            public void Run(VM vm)
            {
                var newEvList = new List<VMEvent>();
                foreach(var ev in _events)
                {
                    if (vm.CurrentTick >= ev.TargetTick)
                    {
                        ev.Event?.Invoke();
                        if (!ev.OnlyOnce)
                            newEvList.Add(ev);
                    }
                    else
                        newEvList.Add(ev);
                }
                _events = newEvList;
            }

            public void AddEvent(VMEvent ev)
            {
                _events.Add(ev);
            }
        }
    }
}

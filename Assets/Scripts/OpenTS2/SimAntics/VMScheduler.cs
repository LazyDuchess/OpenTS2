using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class VMScheduler
    {
        public bool RunningTick => _runningTick;
        private bool _runningTick = false;

        private VMScheduledEvents _preTickEvents = new VMScheduledEvents();
        private VMScheduledEvents _postTickEvents = new VMScheduledEvents();

        /// <summary>
        /// Runs a function immediately if the VM is not currently in a tick, otherwise schedules it to run at the end of the current tick.
        /// </summary>
        public void ScheduleWhenPossible(Action func)
        {
            if (!RunningTick)
            {
                func.Invoke();
                return;
            }
            ScheduleOnEndTick(func);
        }

        /// <summary>
        /// Schedules a function to run once the VM begins a tick.
        /// </summary>
        public void ScheduleOnBeginTick(Action func, uint targetTick = 0)
        {
            _preTickEvents.AddEvent(new VMEvent(func, targetTick));
        }

        /// <summary>
        /// Schedules a function to run once the VM is done with a tick.
        /// </summary>
        public void ScheduleOnEndTick(Action func, uint targetTick = 0)
        {
            _postTickEvents.AddEvent(new VMEvent(func, targetTick));
        }

        // TODO - Right now the way this works is that once an entity has been notified out of idle/interrupted, the first idle that gets executed on the next tick (or that continues to run if there is one already running) won't actually sleep. If no idles are ran then the interrupt is just discarded. Verify this is okay!
        public void ScheduleInterrupt(VMThread thread)
        {
            ScheduleWhenPossible(() =>
            {
                thread.Interrupt = true;
            });
        }

        public void ScheduleDeletion(VMEntity entity)
        {
            ScheduleWhenPossible(() =>
            {
                entity.Delete(); 
            });
            entity.PendingDeletion = true;
        }

        public void OnBeginTick(VM vm)
        {
            _preTickEvents.Run(vm);
            _runningTick = true;
        }

        public void OnEndTick(VM vm)
        {
            _runningTick = false;
            _postTickEvents.Run(vm);
        }

        public class VMEvent
        {
            public Action Event;
            public uint TargetTick = 0;

            public VMEvent(Action func, uint targetTick = 0)
            {
                Event = func;
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

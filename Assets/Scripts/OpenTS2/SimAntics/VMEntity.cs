using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// This is a process running in the SimAntics virtual machine.
    /// </summary>
    public class VMEntity
    {
        public bool PendingDeletion = false;
        public short ID = 1;
        public short[] Temps = new short[20];
        // Main thread the VM will tick.
        public VMThread MainThread;
        public VM VM;
        public ObjectDefinitionAsset ObjectDefinition;
        public uint PrivateGroupID => ObjectDefinition.GlobalTGI.GroupID;
        public uint SemiGlobalGroupID
        {
            get
            {
                var semiGlobal = ObjectDefinition.SemiGlobal;
                if (semiGlobal == null)
                    return 0;
                return semiGlobal.SemiGlobalGroupID;
            }
        }

        protected VMEntity()
        {
            MainThread = new VMThread(this);
        }

        public VMEntity(ObjectDefinitionAsset objectDefinition) : this()
        {
            ObjectDefinition = objectDefinition;
        }

        public void Tick()
        {
            MainThread.Tick();
        }

        public void Delete()
        {
            if (VM.Scheduler.RunningTick)
            {
                if (PendingDeletion)
                    return;
                VM.Scheduler.ScheduleDeletion(this);
                return;
            }
            VM.RemoveEntity(ID);
        }
    }
}

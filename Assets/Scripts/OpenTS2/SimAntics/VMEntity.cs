using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// This is a process or thread running in the SimAntics virtual machine, with its own stack and temp variables.
    /// </summary>
    public class VMEntity
    {
        public short ID = 1;
        public short[] Temps = new short[20];
        public VMStack Stack;
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
            Stack = new VMStack(this);
        }

        public VMEntity(ObjectDefinitionAsset objectDefinition) : this()
        {
            ObjectDefinition = objectDefinition;
        }

        public void Tick()
        {
            Stack.Tick();
        }
    }
}

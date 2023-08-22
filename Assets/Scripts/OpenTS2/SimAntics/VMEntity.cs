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
        public VMEntity(VM vm)
        {
            VM = vm;
            Stack = new VMStack(this);
            ID = vm.GetUniqueID();
        }
        public void Tick()
        {
            Stack.Tick();
        }
    }
}

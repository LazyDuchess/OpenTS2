using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.SimAntics
{
    public class VM
    {
        public List<VMEntity> Entities = new List<VMEntity>();
        public uint CurrentTick = 0;
        public void Tick()
        {
            foreach(var entity in Entities)
            {
                entity.Tick();
            }
            CurrentTick++;
        }
    }
}

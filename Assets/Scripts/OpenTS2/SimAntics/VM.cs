using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// SimAntics virtual machine.
    /// </summary>
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

        public static BHAVAsset GetBHAV(ushort id, uint groupID)
        {
            return ContentProvider.Get().GetAsset<BHAVAsset>(new ResourceKey(id, groupID, TypeIDs.BHAV));
        }
    }
}

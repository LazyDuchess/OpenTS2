using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// SimAntics virtual machine.
    /// </summary>
    public class VM
    {
        public VMScheduler Scheduler = new VMScheduler();
        public List<VMEntity> Entities = new List<VMEntity>();
        public uint CurrentTick = 0;

        private Dictionary<short, VMEntity> _entitiesByID = new Dictionary<short, VMEntity>();

        /// <summary>
        /// Ticks all entities and advances the Simulation by 1 tick.
        /// </summary>
        public void Tick()
        {
            Scheduler.OnBeginTick(this);
            foreach(var entity in Entities)
            {
                entity.Tick();
            }
            Scheduler.OnEndTick(this);
            CurrentTick++;
        }

        /// <summary>
        /// Retrieves a BHAV Asset from the content system.
        /// </summary>
        public static BHAVAsset GetBHAV(ushort id, uint groupID)
        {
            return ContentProvider.Get().GetAsset<BHAVAsset>(new ResourceKey(id, groupID, TypeIDs.BHAV));
        }

        public VMEntity GetEntityByID(short id)
        {
            if (_entitiesByID.TryGetValue(id, out VMEntity result))
                return result;
            return null;
        }

        public void AddEntity(VMEntity entity)
        {
            Entities.Add(entity);
            _entitiesByID[entity.ID] = entity;
        }

        /// <summary>
        /// Returns a free unused entity ID.
        /// </summary>
        public short GetUniqueID()
        {
            short resultID = 1;
            while (_entitiesByID.TryGetValue(resultID, out VMEntity _))
                resultID++;
            return resultID;
        }
    }
}

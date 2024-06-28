using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System;
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
        public short[] GlobalState;
        public VMScheduler Scheduler = new VMScheduler();
        public List<VMEntity> Entities = new List<VMEntity>();
        public uint CurrentTick = 0;
        public Action<Exception, VMEntity> ExceptionHandler;

        private Dictionary<short, VMEntity> _entitiesByID = new Dictionary<short, VMEntity>();

        public VM()
        {
            GlobalState = new short[60];
            InitializeGlobalState();
        }

        public short GetGlobal(ushort id)
        {
            return GlobalState[id];
        }

        public void SetGlobal(ushort id, short value)
        {
            GlobalState[id] = value;
        }

        public short GetGlobal(VMGlobals global)
        {
            return GetGlobal((ushort)global);
        }

        public void SetGlobal(VMGlobals global, short value)
        {
            SetGlobal((ushort)global, value);
        }

        void InitializeGlobalState()
        {
            var epManager = EPManager.Instance;
            var epFlags1 = (short)(epManager.InstalledProducts & 0xFFFF);
            var epFlags2 = (short)(epManager.InstalledProducts >> 16);
            SetGlobal(VMGlobals.GameEditionFlags1, epFlags1);
            SetGlobal(VMGlobals.GameEditionFlags2, epFlags2);
            var settings = Settings.Instance;
            SetGlobal(VMGlobals.CurrentLanguage, (short)settings.Language);
        }

        /// <summary>
        /// Ticks all entities and advances the Simulation by 1 tick.
        /// </summary>
        public void Tick()
        {
            Scheduler.OnBeginTick(this);
            foreach(var entity in Entities)
            {
                try
                {
                    entity.Tick();
                }
                catch(Exception e)
                {
                    ExceptionHandler?.Invoke(e, entity);
                }
            }
            Scheduler.OnEndTick(this);
            CurrentTick++;
        }

        /// <summary>
        /// Retrieves a BHAV Asset from the content system.
        /// </summary>
        public static BHAVAsset GetBHAV(ushort id, uint groupID)
        {
            return ContentManager.Instance.GetAsset<BHAVAsset>(new ResourceKey(id, groupID, TypeIDs.BHAV));
        }

        public VMEntity GetEntityByID(short id)
        {
            if (_entitiesByID.TryGetValue(id, out VMEntity result))
                return result;
            return null;
        }

        /// <summary>
        /// Adds an entity to the simulator, and assigns a unique ID to it.
        /// </summary>
        /// <param name="entity"></param>
        public void AddEntity(VMEntity entity)
        {
            entity.VM = this;
            entity.ID = GetUniqueID();
            Entities.Add(entity);
            _entitiesByID[entity.ID] = entity;
        }

        public void RemoveEntity(short id)
        {
            if (!_entitiesByID.TryGetValue(id, out VMEntity result))
                return;
            _entitiesByID.Remove(id);
            Entities.Remove(result);
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

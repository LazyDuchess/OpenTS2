using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Lua.Disassembly.OpCodes;
using System;
using System.Collections.Generic;
using System.IO;
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
        public short[] Attributes;
        public short[] SemiAttributes;
        public short[] ObjectData = new short[114];
        public uint PrivateGroupID => ObjectDefinition.GlobalTGI.GroupID;
        public uint SemiGlobalGroupID
        {
            get
            {
                var semiGlobal = ObjectDefinition.SemiGlobal;
                if (semiGlobal == null)
                    throw new FileNotFoundException($"Object {ObjectDefinition.FileName} ({ObjectDefinition.GlobalTGI}) has no semi-global file");
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
            Attributes = new short[GetNumberOfAttributes()];
            SemiAttributes = new short[GetNumberOfSemiGlobalAttributes()];
        }

        // TODO: Verify these two methods below are right. Sometimes objdefs have 0 attributes but do have attribute labels and get/set their attributes.
        private int GetNumberOfAttributes()
        {
            var objDefAttributes = ObjectDefinition.NumAttributes;
            var attrLabels = ContentManager.Instance.GetAsset<StringSetAsset>(new ResourceKey(0x100, PrivateGroupID, TypeIDs.STR));
            if (attrLabels == null)
                return objDefAttributes;
            var attrLabelsCount = attrLabels.StringData.Strings[Languages.USEnglish].Count;
            return attrLabelsCount > objDefAttributes ? attrLabelsCount : objDefAttributes;
        }

        private int GetNumberOfSemiGlobalAttributes()
        {
            var semiGlobal = ObjectDefinition.SemiGlobal;
            if (semiGlobal == null)
                return 0;
            var semiAttributeLabels = ContentManager.Instance.GetAsset<StringSetAsset>(new ResourceKey(0x100, semiGlobal.SemiGlobalGroupID, TypeIDs.STR));
            if (semiAttributeLabels == null)
                return 0;
            return semiAttributeLabels.StringData.Strings[Languages.USEnglish].Count;
        }

        public short GetObjectData(VMObjectData field)
        {
            return ObjectData[(int)field];
        }

        public void ClearObjectFlags(VMObjectData field, short value)
        {
            ObjectData[(int)field] = (short)(ObjectData[(int)field] ^ value);
        }

        public void SetObjectFlags(VMObjectData field, short value)
        {
            ObjectData[(int)field] = (short)(ObjectData[(int)field] | value);
        }

        public void SetObjectData(VMObjectData field, short value)
        {
            ObjectData[(int)field] = value;
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

        public BHAVAsset GetBHAV(ushort treeID)
        {
            // 0x0XXX is global scope, 0x1XXX is private scope and 0x2XXX is semiglobal scope.
            var groupid = SemiGlobalGroupID;

            if (treeID < 0x1000)
                groupid = GroupIDs.Global;
            else if (treeID < 0x2000)
                groupid = PrivateGroupID;

            return VM.GetBHAV(treeID, groupid);
        }

        public void PushTreeToThread(VMThread thread, ushort treeId)
        {
            var bhav = GetBHAV(treeId);
            var stackFrame = new VMStackFrame(bhav, thread);
            thread.Frames.Push(stackFrame);
        }

        public VMExitCode RunTreeImmediately(ushort treeID)
        {
            var thread = new VMThread(this);
            thread.CanYield = false;

            var bhav = GetBHAV(treeID);

            var stackFrame = new VMStackFrame(bhav, thread);
            thread.Frames.Push(stackFrame);

            return thread.Tick();
        }
    }
}

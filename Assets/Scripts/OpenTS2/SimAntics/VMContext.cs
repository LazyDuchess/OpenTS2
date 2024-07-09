using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    /// <summary>
    /// SimAntics virtual machine context to be sent to primitives and such.
    /// </summary>
    public struct VMContext
    {
        public VMStackFrame StackFrame;
        public BHAVAsset.Node Node;
        public VMThread Thread => StackFrame.Thread;
        public VMEntity Entity => StackFrame.Thread.Entity;
        public VMEntity StackObjectEntity => VM.GetEntityByID(StackFrame.StackObjectID);
        public VM VM => Entity.VM;

        // TODO - still super incomplete, just enough to run basic scripts.
        public short GetData(VMDataSource source, short dataIndex)
        {
            try
            {
                return source switch
                {
                    VMDataSource.Globals => VM.GetGlobal((ushort)dataIndex),
                    VMDataSource.MyObjectsAttributes => Entity.Attributes[dataIndex],
                    VMDataSource.MyObjectsSemiAttributes => Entity.SemiAttributes[dataIndex],
                    VMDataSource.MyObject => Entity.ObjectData[dataIndex],
                    VMDataSource.StackObject => StackObjectEntity.ObjectData[dataIndex],
                    VMDataSource.Literal => dataIndex,
                    VMDataSource.Temps => Entity.Temps[dataIndex],
                    VMDataSource.Params => StackFrame.Arguments[dataIndex],
                    VMDataSource.StackObjectID => StackFrame.StackObjectID,
                    VMDataSource.TempByTempIndex => Entity.Temps[Entity.Temps[dataIndex]],
                    VMDataSource.StackObjectsTemp => StackObjectEntity.Temps[dataIndex],
                    VMDataSource.Local => StackFrame.Locals[dataIndex],
                    VMDataSource.StackObjectsDefinition => (short)StackObjectEntity.ObjectDefinition.Fields[dataIndex],
                    VMDataSource.StackObjectsAttributes => StackObjectEntity.Attributes[dataIndex],
                    VMDataSource.StackObjectsSemiAttributes => StackObjectEntity.SemiAttributes[dataIndex],
                    VMDataSource.StackObjectsSemiAttributeByParam => StackObjectEntity.SemiAttributes[StackFrame.Arguments[dataIndex]],
                    _ => throw new SimAnticsException($"Attempted to retrieve a variable from an out of range data source ({source}[{dataIndex}]).", StackFrame)
                };
            }
            catch (IndexOutOfRangeException)
            {
                throw new SimAnticsException($"Attempted to retrieve a variable from an out of range data index ({source}[{dataIndex}]).", StackFrame);
            }
        }

        public void SetData(VMDataSource source, short dataIndex, short value)
        {
            try
            {
                switch (source)
                {
                    case VMDataSource.Globals:
                        VM.SetGlobal((ushort)dataIndex, value);
                        return;
                    case VMDataSource.MyObjectsAttributes:
                        Entity.Attributes[dataIndex] = value;
                        return;
                    case VMDataSource.MyObjectsSemiAttributes:
                        Entity.SemiAttributes[dataIndex] = value;
                        return;
                    case VMDataSource.MyObject:
                        Entity.ObjectData[dataIndex] = value;
                        return;
                    case VMDataSource.StackObject:
                        StackObjectEntity.ObjectData[dataIndex] = value;
                        return;
                    case VMDataSource.Temps:
                        Entity.Temps[dataIndex] = value;
                        return;
                    case VMDataSource.Params:
                        StackFrame.Arguments[dataIndex] = value;
                        return;
                    case VMDataSource.StackObjectID:
                        StackFrame.StackObjectID = value;
                        return;
                    case VMDataSource.TempByTempIndex:
                        Entity.Temps[Entity.Temps[dataIndex]] = value;
                        return;
                    case VMDataSource.StackObjectsTemp:
                        StackObjectEntity.Temps[dataIndex] = value;
                        return;
                    case VMDataSource.Local:
                        StackFrame.Locals[dataIndex] = value;
                        return;
                    case VMDataSource.StackObjectsAttributes:
                        StackObjectEntity.Attributes[dataIndex] = value;
                        return;
                    case VMDataSource.StackObjectsSemiAttributes:
                        StackObjectEntity.SemiAttributes[dataIndex] = value;
                        return;
                    case VMDataSource.StackObjectsSemiAttributeByParam:
                        StackObjectEntity.SemiAttributes[StackFrame.Arguments[dataIndex]] = value;
                        return;
                }
                throw new SimAnticsException($"Attempted to modify a variable from an out of range data source ({source}[{dataIndex}]).", StackFrame);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new SimAnticsException($"Attempted to modify a variable from an out of range data index ({source}[{dataIndex}]).", StackFrame);
            }
        }
    }
}

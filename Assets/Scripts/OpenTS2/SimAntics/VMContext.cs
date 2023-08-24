﻿using System;
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
        public VMStack Stack => StackFrame.Stack;
        public VMEntity Entity => StackFrame.Stack.Entity;
        public VMEntity StackObjectEntity => VM.GetEntityByID(StackFrame.StackObjectID);
        public VM VM => Entity.VM;

        // TODO - still super incomplete, just enough to run basic scripts.
        public short GetData(VMDataSource source, short dataIndex)
        {
            return source switch
            {
                VMDataSource.Literal => dataIndex,
                VMDataSource.Temps => Entity.Temps[dataIndex],
                VMDataSource.Params => StackFrame.Arguments[dataIndex],
                VMDataSource.StackObjectID => StackFrame.StackObjectID,
                VMDataSource.TempByTempIndex => Entity.Temps[Entity.Temps[dataIndex]],
                VMDataSource.StackObjectsTemp => StackObjectEntity.Temps[dataIndex],
                VMDataSource.Local => StackFrame.Locals[dataIndex],
                VMDataSource.StackObjectsDefinition => (short)StackObjectEntity.ObjectDefinition.Fields[dataIndex],
                _ => throw new SimAnticsException("Attempted to retrieve a variable from an unknown data source.", StackFrame)
            };
        }

        public void SetData(VMDataSource source, short dataIndex, short value)
        {
            switch(source)
            {
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
            }
            throw new SimAnticsException("Attempted to modify a variable from an unknown data source.", StackFrame);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public enum VMDataSource : byte
    {
        MyObjectsAttributes,
        StackObjectsAttributes,
        MyObjectsSemiAttributes,
        MyObject,
        StackObject,
        StackObjectsSemiAttributes,
        Globals,
        Literal,
        Temps,
        Params,
        StackObjectID,
        // Temp[Temp]
        TempByTempIndex,
        CheckTreeAdRange,
        StackObjectsTemp,
        MyMotives,
        StackObjectsMotives,
        StackObjectsSlot,
        // Stack obj's motive[temp]
        StackObjectsMotiveByTemp,
        MyPersonData,
        StackObjectsPersonData,
        MySlot,
        StackObjectsDefinition,
        // Stack Objs Attribute[Param]
        StackObjectsAttributeByParam,
        // Room[Temp0]
        RoomInTemp0,
        NeighborInStackObject,
        Local,
        Constant,
        Unused,
        CheckTreeAdPersonalityVar,
        CheckTreeAdMin,
        // My Person Data[Temp]
        MyPersonDataByTemp,
        // Stack Obj's person data [Temp]
        StackObjectsPersonDataByTemp,
        NeighborsPersonData,
        // Job data [temp0,1]
        JobDataByTemp0And1,
        NeighborhoodDataReadOnly,
        StackObjectsFunction,
        MyTypeAttribute,
        StackObjectsTypeAttribute,
        NeighborsDefinition,
        MyTempToken,
        StackObjectsTempToken,
        // My object array [array] Iterator Index
        MyObjectsArrayByArrayIteratorIndex,
        // Stack Object's object array [array] iterator Index
        StackObjectsArrayByArrayIteratorIndex,
        // My object array [array] iterator data
        MyObjectsArrayByArrayIteratorData,
        // Stack Object's object array [array] iterator Data
        StackObjectsArrayByArrayIteratorData,
        // My object array [array] element at Temp0
        MyObjectsArrayByArrayElementAtTemp0,
        // Stack Object's object array [array] element at Temp0
        StackObjectsArrayByArrayElementAtTemp0,
        // Const[Temp]
        ConstByTemp,
        // My slot[Temp]
        MySlotByTemp,
        // Stack Object's slot[Temp]
        StackObjectsSlotByTemp,
        // Stack Object's semi attr[Param]
        StackObjectsSemiAttributeByParam,
        StackObjectsMasterDefinition
    }
}

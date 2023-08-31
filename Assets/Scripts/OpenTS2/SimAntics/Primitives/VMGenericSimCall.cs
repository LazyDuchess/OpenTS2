using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    public class VMGenericSimCall : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var callID = ctx.Node.GetOperand(0);

            switch(callID)
            {
                // NodeConsolidation_GlobalInitObject
                case 0x41:
                    ctx.Entity.SetObjectData(VMObjectData.RotationNotches, 2);
                    ctx.Entity.SetObjectData(VMObjectData.RoomPlacement, 0);
                    // Bit 1 floor
                    ctx.Entity.SetObjectFlags(VMObjectData.AllowedHeightFlags, 1);
                    // Bit 6 burns
                    ctx.Entity.SetObjectFlags(VMObjectData.FlagField2, 32);
                    ctx.Entity.SetObjectData(VMObjectData.TrashUnits, 1);
                    // Bit 1 floor, bit 2 terrain
                    ctx.Entity.SetObjectFlags(VMObjectData.PlacementFlags, 1 | 2);
                    // Bit 2 player can move, Bit 4 player can delete
                    ctx.Entity.SetObjectFlags(VMObjectData.MovementFlags, 8 | 2);
                    // Bit 14 can't be billed
                    ctx.Entity.ClearObjectFlags(VMObjectData.FlagField2, 8192);
                    ctx.Entity.SetObjectData(VMObjectData.LookAtScore, 5);
                    ctx.StackFrame.StackObjectID = ctx.Entity.ID;
                    return VMReturnValue.ReturnTrue;
            }
            throw new SimAnticsException($"Invalid Generic Sim Call ID {callID}", ctx.StackFrame);
        }
    }
}

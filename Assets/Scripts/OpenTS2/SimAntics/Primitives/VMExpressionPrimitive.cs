using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    /// <summary>
    /// Expression Primitive, handles variable assignation, retrieval, math.
    /// </summary>
    public class VMExpressionPrimitive : VMPrimitive
    {
        public enum Operator : byte
        {
            GreaterThan,
            LessThan,
            EqualTo,
            Add,
            Subtract,
            Assign,
            Multiply,
            Divide,
            IsFlagSet,
            SetFlag,
            ClearFlag,
            IncThenLessThan,
            Modulo,
            And,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,
            NotEqualTo,
            DecThenGreaterThan,
            Or,
            Xor,
            Abs,
            Assign32BitValue
        }
        public override VMReturnValue Execute(VMContext ctx)
        {
            var lhsData = ctx.Node.GetUInt16(0);
            var rhsData = ctx.Node.GetUInt16(2);
            var signedFlag = ctx.Node.Operands[4];
            var op = (Operator)ctx.Node.Operands[5];
            var lhsSource = (VMDataSource)ctx.Node.Operands[6];
            var rhsSource = (VMDataSource)ctx.Node.Operands[7];

            short lhs, rhs;

            switch(op)
            {
                case Operator.GreaterThan:
                    return ctx.GetData(lhsSource, lhsData) > ctx.GetData(rhsSource, rhsData) ? 
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.LessThan:
                    return ctx.GetData(lhsSource, lhsData) < ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.EqualTo:
                    return ctx.GetData(lhsSource, lhsData) == ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.Add:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, (short)(lhs + rhs));
                    return VMReturnValue.ReturnTrue;
                case Operator.Subtract:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, (short)(lhs - rhs));
                    return VMReturnValue.ReturnTrue;
                case Operator.Assign:
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, rhs);
                    return VMReturnValue.ReturnTrue;
                case Operator.Multiply:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, (short)(lhs * rhs));
                    return VMReturnValue.ReturnTrue;
                case Operator.Divide:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, (short)(lhs / rhs));
                    return VMReturnValue.ReturnTrue;
                case Operator.IsFlagSet:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    return ((lhs & (1 << (rhs - 1))) > 0) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.SetFlag:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var bitval = 1 << (rhs - 1);
                    var finalSet = (int)lhs | bitval;
                    ctx.SetData(lhsSource, lhsData, (short)(finalSet));
                    return VMReturnValue.ReturnTrue;
                case Operator.ClearFlag:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var clearBitVal = ~(1 << (rhs - 1));
                    var finalClear = (int)lhs & clearBitVal;
                    ctx.SetData(lhsSource, lhsData, (short)(finalClear));
                    return VMReturnValue.ReturnTrue;
                case Operator.IncThenLessThan:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsAdded = lhs + 1;
                    ctx.SetData(lhsSource, lhsData, (short)lhsAdded);
                    return lhsAdded < rhs ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.Modulo:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsMod = lhs % rhs;
                    ctx.SetData(lhsSource, lhsData, (short)lhsMod);
                    return VMReturnValue.ReturnTrue;
                case Operator.And:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsAnd = lhs & rhs;
                    ctx.SetData(lhsSource, lhsData, (short)lhsAnd);
                    return VMReturnValue.ReturnTrue;
                case Operator.GreaterThanOrEqualTo:
                    return ctx.GetData(lhsSource, lhsData) >= ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.LessThanOrEqualTo:
                    return ctx.GetData(lhsSource, lhsData) <= ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.NotEqualTo:
                    return ctx.GetData(lhsSource, lhsData) != ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.DecThenGreaterThan:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsSubbed = lhs - 1;
                    ctx.SetData(lhsSource, lhsData, (short)lhsSubbed);
                    return lhsSubbed > rhs ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.Or:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsOr = lhs | rhs;
                    ctx.SetData(lhsSource, lhsData, (short)lhsOr);
                    return VMReturnValue.ReturnTrue;
                case Operator.Xor:
                    lhs = ctx.GetData(lhsSource, lhsData);
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var lhsXor = lhs ^ rhs;
                    ctx.SetData(lhsSource, lhsData, (short)lhsXor);
                    return VMReturnValue.ReturnTrue;
                case Operator.Abs:
                    rhs = ctx.GetData(rhsSource, rhsData);
                    ctx.SetData(lhsSource, lhsData, Math.Abs(rhs));
                    return VMReturnValue.ReturnTrue;
                // Check this as it's new in TS2.
                case Operator.Assign32BitValue:
                    rhs = ctx.GetData(rhsSource, rhsData);
                    var rhs2 = ctx.GetData(rhsSource, (ushort)(rhsData+1));
                    ctx.SetData(lhsSource, lhsData, rhs);
                    ctx.SetData(lhsSource, (ushort)(lhsData+1), rhs2);
                    return VMReturnValue.ReturnTrue;
            }
            return VMReturnValue.ReturnFalse;
        }
    }
}

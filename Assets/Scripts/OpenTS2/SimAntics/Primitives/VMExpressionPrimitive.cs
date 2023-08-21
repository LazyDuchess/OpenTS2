using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
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
            AddThenLessThan,
            Modulo,
            And,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,
            NotEqualTo,
            SubtractThenGreaterThan,
            Or,
            Xor,
            Abs,
            Assign32BitValue
        }
        public override VMReturnValue Execute(VMPrimitiveContext ctx)
        {
            var lhsData = ctx.Node.GetUInt16(0);
            var rhsData = ctx.Node.GetUInt16(2);
            var signedFlag = ctx.Node.Operands[4];
            var op = (Operator)ctx.Node.Operands[5];
            var lhsSource = (VMDataSource)ctx.Node.Operands[6];
            var rhsSource = (VMDataSource)ctx.Node.Operands[7];

            switch(op)
            {
                case Operator.GreaterThan:
                    return ctx.GetData(lhsSource, lhsData) > ctx.GetData(rhsSource, rhsData) ? 
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
                case Operator.LessThan:
                    return ctx.GetData(lhsSource, lhsData) < ctx.GetData(rhsSource, rhsData) ?
                        VMReturnValue.ReturnTrue : VMReturnValue.ReturnFalse;
            }
            return VMReturnValue.ReturnFalse;
        }
    }
}

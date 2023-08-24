using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    public class VMRandomNumber : VMPrimitive
    {
        public override VMReturnValue Execute(VMContext ctx)
        {
            var lhsSource = (VMDataSource)ctx.Node.GetOperand(2);
            var lhsData = ctx.Node.GetInt16Operand(0);

            var rhsSource = (VMDataSource)ctx.Node.GetOperand(6);
            var rhsData = ctx.Node.GetInt16Operand(4);

            var randomMaxValue = ctx.GetData(rhsSource, rhsData);
            var randomFinalValue = (short)UnityEngine.Random.Range(0, randomMaxValue);

            ctx.SetData(lhsSource, lhsData, randomFinalValue);

            return VMReturnValue.ReturnTrue;
        }
    }
}

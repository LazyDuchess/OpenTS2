using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics.Primitives
{
    public static class VMPrimitiveRegistry
    {
        private static Dictionary<ushort, VMPrimitive> s_primitiveByOpCode = new Dictionary<ushort, VMPrimitive>();
        
        public static void Initialize()
        {
            RegisterPrimitive<VMSleep>(0x0);
            RegisterPrimitive<VMGenericSimCall>(0x1);
            RegisterPrimitive<VMExpression>(0x2);
            RegisterPrimitive<VMRandomNumber>(0x8);
            RegisterPrimitive<VMRemoveObjectInstance>(0x12);
            RegisterPrimitive<VMNotifyStackObjectOutOfIdle>(0x31);
            RegisterPrimitive<VMLua>(0x7E);
        }
        
        public static void RegisterPrimitive<T>(ushort opcode) where T : VMPrimitive
        {
            s_primitiveByOpCode[opcode] = Activator.CreateInstance(typeof(T)) as VMPrimitive;
        }

        public static T GetPrimitive<T>(ushort opcode) where T : VMPrimitive
        {
            return GetPrimitive(opcode) as T;
        }

        public static VMPrimitive GetPrimitive(ushort opcode)
        {
            if (s_primitiveByOpCode.TryGetValue(opcode, out VMPrimitive returnPrim))
                return returnPrim;
            return null;
        }
    }
}

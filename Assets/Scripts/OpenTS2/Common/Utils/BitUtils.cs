using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Common.Utils
{
    public static class BitUtils
    {
        public static bool AllBitsSet(int value, int bitMask)
        {
            if ((value & bitMask) == bitMask)
                return true;
            return false;
        }
        public static bool AnyBitsSet(int value, int bitMask)
        {
            if ((value & bitMask) != 0)
                return true;
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public enum Product : byte
    {
        BaseGame = 0,
        University = 1,
        Nightlife = 2,
        Business = 3,
        FamilyFun = 4,
        Glamour = 5,
        Pets = 6,
        Seasons = 7,
        Celebration = 8,
        HnM = 9,
        BonVoyage = 10,
        Teen = 11,
        FreeTime = 13,
        Kitchen = 14,
        IKEA = 15,
        ApartmentLife = 16,
        Mansion = 17
    }
    public static class EPSupport
    {
        public static int GetEPFlagFromProduct(Product product)
        {
            return GetEPFlagFromProductIndex((byte)product);
        }
        static int GetEPFlagFromProductIndex(byte productIndex)
        {
            return 1 << (productIndex & 0x1f);
        }
    }
}

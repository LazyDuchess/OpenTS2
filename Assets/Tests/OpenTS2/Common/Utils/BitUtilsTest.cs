using NUnit.Framework;
using OpenTS2.Common.Utils;


public class BitUtilsTest
{
    [Test]
    public void AllBitsSetReturnsTrueWithSimpleMask()
    {
        Assert.IsTrue(BitUtils.AllBitsSet(1, 1));
    }

    [Test]
    public void AllBitSetReturnsFalseWithSimpleMask()
    {
        Assert.IsFalse(BitUtils.AllBitsSet(0, 1));
    }
}

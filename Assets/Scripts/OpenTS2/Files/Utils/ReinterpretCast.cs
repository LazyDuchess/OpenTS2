using Unity.Collections.LowLevel.Unsafe;

namespace OpenTS2.Files.Utils
{
    public unsafe static class ReinterpretCast
    {
        public static void Copy<T1, T2>(T1[] src, T2[] dst, int bytes) where T1 : unmanaged where T2 : unmanaged
        {
            fixed (void* srcPtr = src, dstPtr = dst)
            {
                UnsafeUtility.MemCpy(dstPtr, srcPtr, bytes);
            }
        }

        public static T2 As<T1, T2>(T1 src) where T1 : unmanaged where T2 : unmanaged
        {
            return *(T2*)&src;
        }
    }
}
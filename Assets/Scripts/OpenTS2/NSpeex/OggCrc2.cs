namespace NSpeex
{
    /// <summary>
    ///  Calculates the CRC checksum for Ogg packets.
    /// Ogg uses the same generator polynomial as ethernet, although with an  unreflected alg and an init/final of 0, not 0xffffffff.
    /// </summary>
    public class OggCrc2
    {
        private static int[] crc_lookup;

        static OggCrc2()
        {
            crc_lookup = new int[256];
            for (int i = 0; i < crc_lookup.Length; i++)
            {
                int r = i << 24;
                for (int j = 0; j < 8; j++)
                {
                    if ((r & 0x80000000) != 0)
                    {
                        /* The same as the ethernet generator polynomial, although we use an
                        unreflected alg and an init/final of 0, not 0xffffffff */
                        r = (r << 1) ^ 0x04c11db7;
                    }
                    else
                    {
                        r <<= 1;
                    }
                }
                crc_lookup[i] = (int)(r & 0xffffffff);
            }
        }

        public static int checksum(int crc,
            byte[] data,
            int offset,
            int length)
        {
            int end = offset + length;
            for (; offset < end; offset++)
            {
                crc = (crc << 8) ^ crc_lookup[((crc >> 24) & 0xff) ^ (data[offset] & 0xff)];
            }
            return crc;
        }
    }
}

using System;
using System.IO;
using System.Text;
namespace NSpeex
{
    /// <summary>
    /// BigEndian
    /// </summary>
    public class BigEndian
    {

        private static byte[] Convert(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                // revert order
                int size = data.Length;
                byte[] result = new byte[size];
                for (int i = 0; i < data.Length; i++)
                {
                    result[i] = data[size-1-i];
                }
                return result;

            }
            else
            {
                return data;
            }
        }

        public static void WriteShort(byte[] buf,int offset,short value)
        {
            // a short has 2 bytes
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            Array.Copy(data2, 0, buf, offset, 2);
        }

        public static void WriteShort(Stream stream,short value)
        {
            // a short has 2 bytes
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            stream.Write(data2,0,data2.Length);
        }


        /// <summary>
        /// write a int value to buf where index is offset,
        /// that means from buf[offset] to buf[offset + 3] is value data
        /// </summary>
        public static void WriteInt(byte[] buf, int offset, int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            Array.Copy(data2, 0, buf, offset, 4);
        }

        public static void WriteInt(Stream stream, int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            stream.Write(data2,0,data2.Length);
        }


        /// <summary>
        /// write a long value to buf where index is offset,
        /// that means from buf[offset] to buf[offset + 7] is value data
        /// </summary>
        public static void WriteLong(byte[] buf, int offset, long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            Array.Copy(data2, 0, buf, offset, 8);
        }

        public static void WriteLong(Stream stream, long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            byte[] data2 = Convert(data);
            stream.Write(data2,0,data2.Length);
        }

        /// <summary>
        /// write a string value to buf where index is offset,
        /// that means from buf[offset] to buf[offset + value.Length] is value data
        /// </summary>
        public static void WriteString(byte[] buf, int offset, string value)
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(value);
            Array.Copy(data, 0, buf, offset, data.Length);
        }

        public static void WriteString(Stream stram,string value)
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(value);
            stram.Write(data,0,data.Length);
        }

        public static short ReadShort(byte[] buf, int offset)
        {
            byte[] data = new byte[2] { 0, 0 };
            Array.Copy(buf, offset, data, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                byte tmp = data[0];
                data[0] = data[1];
                data[1] = tmp;
            }
            return BitConverter.ToInt16(data, 0);
        }
        public static short ReadShort(Stream stream)
        {
            byte[] data = new byte[2] { 0, 0 };
            stream.Read(data, 0, 2);
            byte[] data2 = Convert(data);
            return BitConverter.ToInt16(data2, 0);
        }
        

        /// <summary>
        /// read value from buf where index is offset
        /// </summary>
        public static int ReadInt(byte[] buf, int offset)
        {
            byte[] data = new byte[4] { 0, 0, 0, 0 };
            Array.Copy(buf, offset, data, 0, 4);
            byte[] data2 = Convert(data);
            return BitConverter.ToInt32(data2, 0);
        }
        public static int ReadInt(Stream stream)
        {
            byte[] data = new byte[4] { 0, 0, 0, 0 };
            stream.Read(data, 0, 4);
            byte[] data2 = Convert(data);
            return BitConverter.ToInt32(data2, 0);
        }


        /// <summary>
        /// read value from buf where index is offset
        /// </summary>
        public static long ReadLong(byte[] buf, int offset)
        {
            byte[] data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Array.Copy(buf, offset, data, 0, 8);
            byte[] data2 = Convert(data);
            return BitConverter.ToInt64(data2, 0);
        }

        public static long ReadLong(Stream stream)
        {
            byte[] data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            stream.Read(data, 0, 8);
            byte[] data2 = Convert(data);
            return BitConverter.ToInt64(data2, 0);
        }

        /// <summary>
        /// read value from buf where index is offset and length is size
        /// </summary>
        public static string ReadString(byte[] buf, int offset, int size)
        {
            return UTF8Encoding.UTF8.GetString(buf, offset, size);
        }

        public static string ReadString(Stream stream,int size)
        {
            byte[] data = new byte[size];
            stream.Read(data, 0, size);
            return UTF8Encoding.UTF8.GetString(data, 0, size);
        }

    }
}

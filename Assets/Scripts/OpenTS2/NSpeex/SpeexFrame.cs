using System;

namespace NSpeex
{
    public class SpeexFrame
    {
        public const int FRAME_MAX_SIZE = 160;//size must less than 160 in byte
        private byte[] mData;
        private int mSize;
        public SpeexFrame(byte[] data, int size)
        {
            this.mData = new byte[size];
            Array.Copy(data, 0, this.mData, 0, size);
            this.mSize = size;
        }
        public byte[] Data
        {
            get { return mData; }
        }
        public int Size
        {
            get { return mSize; }
        }
        public static SpeexFrame ParseFrom(byte[] buf, int offset, int size)
        {
            byte[] data = new byte[size];
            Array.Copy(buf, offset, data, 0, size);
            return new SpeexFrame(data, size);
        }

    }
}

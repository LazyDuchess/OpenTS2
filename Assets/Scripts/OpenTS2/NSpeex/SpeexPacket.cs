using System;

namespace NSpeex
{
    public class SpeexPacket
    {
        public int FramePerPacket { get; set; }

        public int FrameSize { get; private set; }


        private byte[] bytes;

        public SpeexPacket(int framePerPacket)
        {
            FramePerPacket = framePerPacket;
            FrameSize = 0;
            Size = 0;
            Data = new byte[SpeexFrame.FRAME_MAX_SIZE * framePerPacket];
        }


        public void Reset()
        {
            FrameSize = 0;
            Size = 0;
        }

        public void AddFrame(SpeexFrame frame)
        {
            if (FrameSize >= FramePerPacket)
            {
                throw new ArgumentException("out of range");
            }
            FrameSize = FrameSize + 1;
            Array.Copy(frame.Data,0,Data,Size,frame.Size);
            Size = Size + frame.Size;

        }

        public int Size { get; private set; }

        public byte[] Data { get; private set; }







    }
}

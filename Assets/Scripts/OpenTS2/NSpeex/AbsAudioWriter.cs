using System.IO;

namespace NSpeex
{
    /// <summary>
    /// abstract audio writer to file
    /// </summary>
    public abstract class AbsAudioWriter
    {
        
        public abstract void Close();

        public abstract void Open(string path);

        public abstract void WriteHeader(string comment);

        public abstract void WritePackage(byte[] data,int offset,int len);

    }
}

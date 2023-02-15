namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphMipLevelInfoAsset : AbstractAsset
    {
        public byte[] MipData { get; }

        public ScenegraphMipLevelInfoAsset(byte[] mipData) => (MipData) = (mipData);
    }
}
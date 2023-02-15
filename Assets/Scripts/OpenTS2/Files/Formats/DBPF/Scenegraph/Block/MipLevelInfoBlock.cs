using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A Scenegraph block representing image data for a single mip level.
    /// </summary>
    public class MipLevelInfoBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xED534136;
        public const string BLOCK_NAME = "cLevelInfo";
        public override string BlockName => BLOCK_NAME;

        public int Width { get; }
        public int Height { get; }

        /// <summary>
        /// Number of bytes in a single row of the image.
        /// </summary>
        public uint Pitch { get; }

        public byte[] Data { get; }

        public MipLevelInfoBlock(PersistTypeInfo blockTypeInfo, int width, int height, uint pitch, byte[] data) : base(
            blockTypeInfo) =>
            (Width, Height, Pitch, Data) = (width, height, pitch, data);
    }

    public class MipLevelInfoBlockReader : IScenegraphDataBlockReader<MipLevelInfoBlock>
    {
        public MipLevelInfoBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var resource = ScenegraphResource.Deserialize(reader);

            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var pitch = reader.ReadUInt32();
            var data = reader.ReadBytes(reader.ReadUInt32());

            return new MipLevelInfoBlock(blockTypeInfo, width, height, pitch, data);
        }
    }
}
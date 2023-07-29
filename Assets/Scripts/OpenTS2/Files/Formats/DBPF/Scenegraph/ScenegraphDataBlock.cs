using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    public abstract class ScenegraphDataBlock
    {
        /// <summary>
        /// The name of the data block as stored in the PersistTypeInfo header. This should be stored as a constant
        /// property.
        ///
        /// For example:
        /// <code>
        /// public const string BLOCK_NAME = "cImageData";
        /// public override string BlockName => BLOCK_NAME;
        /// </code>
        /// </summary>
        public abstract string BlockName { get; }


        /// <summary>
        /// The type and version of this block.
        /// </summary>
        public PersistTypeInfo BlockTypeInfo { get; }
        protected ScenegraphDataBlock(PersistTypeInfo blockTypeInfo) => (BlockTypeInfo) = (blockTypeInfo);


        public override string ToString()
        {
            return $"{BlockTypeInfo.Name} v={BlockTypeInfo.Version}";
        }
    }

    /// <summary>
    /// Factory class to implement deserialization for Scenegraph data blocks.
    /// </summary>
    /// <typeparam name="T">The ScenegraphDataBlock this class deserializes.</typeparam>
    public interface IScenegraphDataBlockReader<out T> where T : ScenegraphDataBlock
    {
        public abstract T Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo);
    }
}
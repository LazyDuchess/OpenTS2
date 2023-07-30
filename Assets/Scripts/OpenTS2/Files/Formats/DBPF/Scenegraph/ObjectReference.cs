using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// A reference to a scenegraph object. This could be stored locally in the scenegraph resource collection or
    /// have an external reference through the FileLinks of a ScenegraphResourceCollection.
    /// </summary>
    public abstract class ObjectReference
    {
        public static ObjectReference Deserialize(IoBuffer reader)
        {
            var referenceMissing = reader.ReadByte() == 0;
            if (referenceMissing)
            {
                return new NullReference();
            }

            var isInternal = reader.ReadByte() == 0;
            var index = reader.ReadInt32();
            if (isInternal)
            {
                return new InternalReference(index);
            }
            return new ExternalReference(index);
        }
    }

    /// <summary>
    /// Not sure if this is null or represents a reference from the current class.
    /// </summary>
    public class NullReference : ObjectReference
    {
    }

    public class InternalReference : ObjectReference
    {
        public int BlockIndex;

        public InternalReference(int blockIndex) => (BlockIndex) = (blockIndex);
    }

    public class ExternalReference : ObjectReference
    {
        public int FileLinksIndex;

        public ExternalReference(int fileLinksIndex) => (FileLinksIndex) = (fileLinksIndex);
    }
}
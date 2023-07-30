using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// A reference to a scenegraph object in the FileLinks of a ScenegraphResourceCollection.
    /// </summary>
    public struct ObjectReference
    {
        public int Index;
        public bool IsInternalReference;

        public static ObjectReference Deserialize(IoBuffer reader)
        {
            var referenceMissing = reader.ReadByte() == 0;
            if (referenceMissing)
            {
                return new ObjectReference() { Index = -1 };
            }

            var isInternal = reader.ReadByte() == 0;
            var index = reader.ReadInt32();
            return new ObjectReference() { Index = index, IsInternalReference = isInternal };
        }
    }
}
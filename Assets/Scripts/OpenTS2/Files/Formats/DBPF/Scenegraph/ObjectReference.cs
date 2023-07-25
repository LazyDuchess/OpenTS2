using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// A reference to a scenegraph object in the FileLinks of a ScenegraphResourceCollection.
    /// </summary>
    public struct ObjectReference
    {
        public int Index;

        public static ObjectReference Deserialize(IoBuffer reader)
        {
            var referenceMissing = reader.ReadByte() == 0;
            if (referenceMissing)
            {
                return new ObjectReference() { Index = -1 };
            }

            // This internally changes the reference between cIGZPersistSerializableReferent when the value is 0 or
            // cIGZPersistResource2 when the value is anything else. We don't care about that yet but we might in the
            // future.
            var referenceType = reader.ReadByte();
            var index = reader.ReadInt32();
            return new ObjectReference() { Index = index };
        }
    }
}
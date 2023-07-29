using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A scenegraph cGeometryNode block.
    ///
    /// These act as indirection to point to a GeometryDataContainerBlock.
    /// </summary>
    public class GeometryNodeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x7BA3838C;
        public const string BLOCK_NAME = "cGeometryNode";
        public override string BlockName => BLOCK_NAME;


        public ScenegraphResource Resource { get; }
        public ObjectReference GeometryDataReference { get; }

        public GeometryNodeBlock(PersistTypeInfo blockTypeInfo, ScenegraphResource resource,
            ObjectReference geometryDataReference) : base(blockTypeInfo)
            => (Resource, GeometryDataReference) = (resource, geometryDataReference);
    }

    public class GeometryNodeBlockReader : IScenegraphDataBlockReader<GeometryNodeBlock>
    {
        public GeometryNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var graphNode = ObjectGraphNodeBlock.Deserialize(reader);
            var resource = ScenegraphResource.Deserialize(reader);

            if (blockTypeInfo.Version > 6)
            {
                var unknownFlag = reader.ReadByte() == 0x0;
            }

            if (blockTypeInfo.Version == 11)
            {
                // Game skips two bytes on version 11.
                reader.ReadBytes(2);
            }

            if (blockTypeInfo.Version == 6)
            {
                // Game reads 3 bools that it does nothing with.
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
            }

            if (blockTypeInfo.Version < 8)
            {
                // We're done.
                return null;
            }

            if (blockTypeInfo.Version < 10)
            {
                // Game skips 12 bytes on versions below 10.
                reader.ReadBytes(12);
            }

            // Read an object reference to the actual geometry data.
            var geometryDataRef = ObjectReference.Deserialize(reader);

            return new GeometryNodeBlock(blockTypeInfo, resource, geometryDataRef);
        }
    }
}
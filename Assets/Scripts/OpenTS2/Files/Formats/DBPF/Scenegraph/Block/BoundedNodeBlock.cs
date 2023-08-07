using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// cBoundedNode, a node that has a bounding box.
    /// </summary>
    public class BoundedNodeBlock
    {
        public TransformNodeBlock Transform { get; }

        public BoundedNodeBlock(TransformNodeBlock transform) => (Transform) = (transform);

        public static BoundedNodeBlock Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cBoundedNode");

            var transform = TransformNodeBlockReader.DeserializeWithoutTypeInfo(reader);

            // ignored boolean, always written as 0.
            var ignoredBool = reader.ReadByte();

            return new BoundedNodeBlock(transform);
        }
    }
}
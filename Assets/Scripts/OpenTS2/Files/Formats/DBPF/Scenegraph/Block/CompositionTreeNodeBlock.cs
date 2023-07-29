using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// cCompositionTreeNode
    /// </summary>
    public class CompositionTreeNodeBlock
    {
        public static CompositionTreeNodeBlock Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            var graph = ObjectGraphNodeBlock.Deserialize(reader);

            var numberOfReferences = reader.ReadUInt32();
            for (var i = 0; i < numberOfReferences; i++)
            {
                var reference = ObjectReference.Deserialize(reader);
            }

            return new CompositionTreeNodeBlock();
        }
    }
}
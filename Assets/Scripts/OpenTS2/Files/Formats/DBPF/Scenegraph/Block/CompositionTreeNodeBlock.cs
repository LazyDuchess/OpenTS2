using System.Linq;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// cCompositionTreeNode
    /// </summary>
    public class CompositionTreeNodeBlock
    {
        public PersistTypeInfo TypeInfo { get; }

        public ObjectReference[] References { get; }

        public CompositionTreeNodeBlock(PersistTypeInfo typeInfo, ObjectReference[] references) =>
            (TypeInfo, References) = (typeInfo, references);

        public static CompositionTreeNodeBlock Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            var graph = ObjectGraphNodeBlock.Deserialize(reader);

            var references = new ObjectReference[reader.ReadUInt32()];
            for (var i = 0; i < references.Length; i++)
            {
                references[i] = ObjectReference.Deserialize(reader);
            }

            return new CompositionTreeNodeBlock(typeInfo, references);
        }

        public override string ToString()
        {
            return $"Type={TypeInfo} References=[{string.Join(", ", References)}]";
        }
    }
}
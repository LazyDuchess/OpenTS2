using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// cRenderableNode
    /// </summary>
    public class RenderableNodeBlock
    {
        public BoundedNodeBlock Bounded { get; }
        public string[] RenderGroups { get; }
        public uint RenderGroupId { get; }
        public bool AddToDisplayList { get; }

        public RenderableNodeBlock(BoundedNodeBlock bounded, string[] renderGroups, uint renderGroupId, bool addToDisplayList) =>
            (Bounded, RenderGroups, RenderGroupId, AddToDisplayList) =
            (bounded, renderGroups, renderGroupId, addToDisplayList);

        public static RenderableNodeBlock Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cRenderableNode");

            var bounded = BoundedNodeBlock.Deserialize(reader);

            var partOfAllRenderGroups = reader.ReadByte() != 0;
            var renderGroups = new string[reader.ReadUInt32()];
            for (var i = 0; i < renderGroups.Length; i++)
            {
                renderGroups[i] = reader.ReadVariableLengthPascalString();
            }

            var renderGroupId = reader.ReadUInt32();
            // kAddToDisplayListMaskBit
            var addToDisplayList = reader.ReadByte() != 0;

            return new RenderableNodeBlock(bounded, renderGroups, renderGroupId, addToDisplayList);
        }
    }
}
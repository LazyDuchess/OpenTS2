using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    public class LightRefNodeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x253d2018;
        public const string BLOCK_NAME = "cLightRefNode";
        public override string BlockName => BLOCK_NAME;

        public RenderableNodeBlock Renderable { get; }

        public ObjectReference Light { get; }

        public LightRefNodeBlock(PersistTypeInfo blockTypeInfo, RenderableNodeBlock renderable, ObjectReference light) : base(blockTypeInfo)
        {
            Renderable = renderable;
            Light = light;
        }
    }

    public class LightRefNodeBlockReader : IScenegraphDataBlockReader<LightRefNodeBlock>
    {
        public LightRefNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var renderable = RenderableNodeBlock.Deserialize(reader);

            ObjectReference light = null;
            if (blockTypeInfo.Version < 10)
            {
                var hasLight = reader.ReadUInt32();
                if (hasLight == 1)
                {
                    light = ObjectReference.Deserialize(reader);
                }

                // Not sure what the second light here and in the other branch is, it seems to be another light
                // reference, maybe with the bounds precomputed or something.
                var secondLightCount = reader.ReadByte();
                var hasSecondLight = reader.ReadByte() != 0;
                if (secondLightCount == 1 && hasSecondLight)
                {
                    var secondLight = ObjectReference.Deserialize(reader);
                }
            }
            else
            {
                light = ObjectReference.Deserialize(reader);

                var secondLightCount = reader.ReadByte();
                var missingPrecomputedLight = reader.ReadByte() != 0;
                if (secondLightCount == 1 && !missingPrecomputedLight)
                {
                    var secondLight = ObjectReference.Deserialize(reader);
                }
            }

            return new LightRefNodeBlock(blockTypeInfo, renderable, light);
        }
    }
}

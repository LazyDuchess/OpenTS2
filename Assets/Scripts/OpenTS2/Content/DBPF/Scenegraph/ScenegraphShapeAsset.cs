using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphShapeAsset : AbstractAsset
    {
        public ShapeBlock ShapeBlock { get; }

        public ScenegraphShapeAsset(ShapeBlock shapeBlock) => (ShapeBlock) = shapeBlock;
    }
}
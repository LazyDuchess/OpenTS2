using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphAnimationAsset : AbstractAsset
    {
        public AnimResourceConstBlock AnimResource { get; }

        public ScenegraphAnimationAsset(AnimResourceConstBlock animResource)
        {
            AnimResource = animResource;
        }
    }
}
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphMaterialDefinitionAsset : AbstractAsset
    {
        public MaterialDefinitionBlock MaterialDefinition { get; }

        public ScenegraphMaterialDefinitionAsset(MaterialDefinitionBlock material) => (MaterialDefinition) = (material);
    }
}
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class TemperateTerrain : TerrainType
    {
        public override string Name => "Temperate";

        public override ResourceKey Texture => new ResourceKey("nh-temperate-wet-00_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Texture1 => new ResourceKey("nh-temperate-wet-01_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);

        public override ResourceKey Roughness => new ResourceKey("nh-temperate-drydry-00_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Roughness1 => new ResourceKey("nh-temperate-drydry-01_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Roughness2 => new ResourceKey("nh-temperate-drydry-02_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
    }
}

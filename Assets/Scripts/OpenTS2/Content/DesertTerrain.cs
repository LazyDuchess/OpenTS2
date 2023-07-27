using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;

namespace OpenTS2.Content
{
    // This is very different from temperate.
    // Texture shows the closest to roads
    // Texture1 is a little further away, but it also shows up on terrain past a certain height on top of Texture.
    // Roughness is far from roads
    // Roughness1 is on the edge transition between Texture1 and Roughness, but also shows up on terrain past a certain height on top of Roughness.
    public class DesertTerrain : TerrainType
    {
        public override string Name => "Desert";

        public override ResourceKey Texture => new ResourceKey("desert-smooth_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Texture1 => new ResourceKey("desert-medium_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);

        public override ResourceKey Roughness => new ResourceKey("desert-rough_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Roughness1 => new ResourceKey("desert-rough-red_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
    }
}

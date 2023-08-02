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
    // Texture1 is a little further away
    // Roughness is far from roads
    // Roughness1 seems to show up on top of Roughness on the sides of hilly terrain, kinda like cliffs but with a different threshold.
    public class DirtTerrain : DesertTerrain
    {
        public override string Name => "Dirt";

        public override ResourceKey Texture => new ResourceKey("dirt-rough_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Texture1 => new ResourceKey("dirt-green_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR);

        public override ResourceKey Roughness => new ResourceKey("dirt-rough-light_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR);
        public override ResourceKey Roughness1 => new ResourceKey("dirt-green-brown_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR);
    }
}

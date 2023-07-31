using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class ImposterTerrainMaterial : AbstractMaterial
    {
        public override string Name => "ImposterTerrainMaterial";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            var textureName = "terrain_txtr";
            var shader = Shader.Find("OpenTS2/LotImposterBlend");
            var material = new Material(shader);

            var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(
                new ResourceKey(textureName, definition.GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
            );
            definition.Textures.Add(texture);
            material.mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get());
            return material;
        }
    }
}

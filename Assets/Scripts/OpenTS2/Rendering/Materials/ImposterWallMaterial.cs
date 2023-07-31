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
    public class ImposterWallMaterial : AbstractMaterial
    {
        public override string Name => "ImposterWallMaterial";

        private static readonly int AlphaCutOff = Shader.PropertyToID("_AlphaCutOff");

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            var textureName = $"walls_{definition.GetProperty("page")}_txtr";
            var shader = Shader.Find("OpenTS2/LotImposterCutOut");
            var material = new Material(shader);

            var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(
                new ResourceKey(textureName, definition.GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
            );
            definition.Textures.Add(texture);
            material.mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get());
            material.mainTexture.filterMode = FilterMode.Point;
            material.SetFloat(AlphaCutOff, 0.5f);
            return material;
        }
    }
}

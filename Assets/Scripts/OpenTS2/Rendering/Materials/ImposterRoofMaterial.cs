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
    public class ImposterRoofMaterial : AbstractMaterial
    {
        public override string Name => "ImposterRoofMaterial";

        private static readonly int Size = Shader.PropertyToID("_Size");

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            var textureName = "roofs_txtr";
            var shader = Shader.Find("OpenTS2/LotImposterRoof");
            var material = new Material(shader);

            var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(
                new ResourceKey(textureName, definition.GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
            );
            definition.Textures.Add(texture);
            material.mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get());
            material.SetFloat(Size, Lot.MaxLotSize);
            return material;
        }
    }
}

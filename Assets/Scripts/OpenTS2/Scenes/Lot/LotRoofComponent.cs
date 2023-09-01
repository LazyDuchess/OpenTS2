using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Scenes.Lot.Roof;
using System.Collections.Generic;

namespace OpenTS2.Scenes.Lot
{
    public class LotRoofComponent : AssetReferenceComponent
    {
        private const uint DefaultGUID = 0x0cdcc049;

        private RoofCollection _roofs;
        private RoofGeometryCollection _geometry;

        public void CreateFromLotAssets(uint patternGuid, RoofCollection roofs)
        {
            _roofs = roofs;

            if (patternGuid == 0)
            {
                patternGuid = DefaultGUID;
            }

            LoadPatterns(patternGuid);
            BuildRoofs();
        }

        private ScenegraphMaterialDefinitionAsset GenerateMaterial(string textureName)
        {
            var persistType = new PersistTypeInfo("cSGResource", 0, 2);

            var mat = new ScenegraphMaterialDefinitionAsset(
                new MaterialDefinitionBlock(persistType,
                    new ScenegraphResource(),
                    textureName,
                    "Floor",
                    new Dictionary<string, string>()
                    {
                        { "deprecatedStdMatInvDiffuseCoeffMultiplier", "1.2" },
                        { "floorMaterialScaleU", "1.000000" },
                        { "floorMaterialScaleV", "1.000000" },
                        { "reflectivity", "0.5" },
                        { "stdMatBaseTextureAddressingU", "tile" },
                        { "stdMatBaseTextureAddressingV", "tile" },
                        { "stdMatBaseTextureEnabled", "true" },
                        { "stdMatBaseTextureName", textureName },
                        { "stdMatDiffCoef", "0.5,0.5,0.5,1" },
                        { "stdMatEmissiveCoef", "0,0,0" },
                        { "stdMatEnvCubeCoef", "0,0,0,0,0" },
                        { "stdMatLayer", "0" },
                        { "stdMatSpecCoef", "0,0,0" },
                        { "stdMatUntexturedDiffAlpha", "1" }
                    },
                    new string[] { textureName }
                )
            );

            mat.TGI = new ResourceKey(textureName + "_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR);

            AddReference(mat);

            return mat;
        }

        public void LoadPatterns(uint guid)
        {
            var contentProvider = ContentProvider.Get();
            var catalogManager = CatalogManager.Get();

            CatalogRoofAsset roof = catalogManager.GetRoofById(guid);

            if (roof == null)
            {
                LoadPatterns(DefaultGUID);
                return;
            }

            _geometry = new RoofGeometryCollection(
                new PatternMesh(gameObject, roof.TextureTop, GenerateMaterial(roof.TextureTop).GetAsUnityMaterial()),
                new PatternMesh(gameObject, roof.TextureEdges, GenerateMaterial(roof.TextureEdges).GetAsUnityMaterial()),
                new PatternMesh(gameObject, roof.TextureTrim, GenerateMaterial(roof.TextureTrim).GetAsUnityMaterial()),
                new PatternMesh(gameObject, roof.TextureUnder, GenerateMaterial(roof.TextureUnder).GetAsUnityMaterial()));
        }

        public void BuildRoofs()
        {
            if (_geometry.Valid())
            {
                _geometry.Clear();

                _roofs.GenerateGeometry(_geometry);

                _geometry.Commit();
            }
        }
    }
}
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
using System;
using OpenTS2.Scenes.Lot.State;

namespace OpenTS2.Scenes.Lot
{
    public class LotRoofComponent : AssetReferenceComponent
    {
        private LotArchitecture _architecture;
        private PatternMeshCollection _patterns;

        public LotRoofComponent CreateFromLotArchitecture(LotArchitecture architecture)
        {
            _architecture = architecture;

            LoadPatterns(architecture.Roof.PatternGUID);
            BuildRoofs();

            return this;
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
                LoadPatterns(RoofCollection.DefaultGUID);
                return;
            }

            _patterns = new PatternMeshCollection(gameObject,
                new PatternDescriptor[]
                {
                    new PatternDescriptor(roof.TextureTop, GenerateMaterial(roof.TextureTop).GetAsUnityMaterial()),
                    new PatternDescriptor(roof.TextureEdges, GenerateMaterial(roof.TextureEdges).GetAsUnityMaterial()),
                    new PatternDescriptor(roof.TextureTrim, GenerateMaterial(roof.TextureTrim).GetAsUnityMaterial()),
                    new PatternDescriptor(roof.TextureUnder, GenerateMaterial(roof.TextureUnder).GetAsUnityMaterial())
                },
                Array.Empty<PatternVariant>(),
                _architecture.FloorPatterns.Depth);
        }

        public void BuildRoofs()
        {
            _patterns.ClearAll();

            _architecture.Roof.GenerateGeometry(_patterns, _architecture.BaseFloor);

            _patterns.CommitAll();
        }

        public void UpdateDisplay(WorldState state)
        {
            _patterns.UpdateDisplay(state, _architecture.BaseFloor, DisplayUpdateType.Roof);
        }
    }
}
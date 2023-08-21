using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenTS2.Scenes.Lot
{
    public class LotFloorComponent : AssetReferenceComponent
    {
        private const string ThicknessTexture = "floor-edge";
        private const string FallbackMaterial = "floor-grid";
        private const float Thickness = 0.1f;

        private class PatternMesh
        {
            public GameObject Object;
            public LotFloorPatternComponent Component;

            public PatternMesh(GameObject parent, string name, Material material)
            {
                Object = new GameObject(name, typeof(LotFloorPatternComponent));
                Component = Object.GetComponent<LotFloorPatternComponent>();

                Object.transform.SetParent(parent.transform);
                Component.Initialize(material);
            }
        }

        private StringMapAsset _patternMap;
        private _3DArrayAsset<float> _elevationData;
        private _3DArrayAsset<Vector4<ushort>> _patternData;
        private int _baseLevel;

        private PatternMesh _thickness;
        private PatternMesh[] _patterns;

        public void CreateFromLotAssets(StringMapAsset patternMap, _3DArrayAsset<Vector4<ushort>> patternData, _3DArrayAsset<float> elevationData, int baseLevel)
        {
            _patternMap = patternMap;
            _patternData = patternData;
            _elevationData = elevationData;
            _baseLevel = baseLevel;

            if (patternData.Width != elevationData.Width - 1 || patternData.Height != elevationData.Height - 1 || patternData.Depth != elevationData.Depth)
            {
                throw new InvalidOperationException("Size mismatch between heightmap and LTTX");
            }

            LoadPatterns();
            BuildFloorMeshes();
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
                        { "stdMatDiffCoef", "0.8,0.8,0.8,1" },
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

        private ScenegraphMaterialDefinitionAsset LoadMaterial(ContentProvider contentProvider, string name)
        {
            var material = contentProvider.GetAsset<ScenegraphMaterialDefinitionAsset>(new ResourceKey($"{name}_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXMT));

            AddReference(material);

            return material;
        }

        private void LoadPatterns()
        {
            // Load the patterns. Some references are by asset name (do not exist in catalog), others are by catalog GUID.

            var contentProvider = ContentProvider.Get();
            var catalogManager = CatalogManager.Get();

            ushort highestId = _patternMap.Map.Keys.Max();
            _patterns = new PatternMesh[highestId + 1];

            foreach (StringMapEntry entry in _patternMap.Map.Values)
            {
                string materialName;
                if (uint.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint guid))
                {
                    var catalogEntry = catalogManager.GetEntryById(guid);

                    materialName = catalogEntry?.Material ?? FallbackMaterial;
                }
                else
                {
                    materialName = entry.Value.StartsWith("floor_") ? entry.Value : ("floor_" + entry.Value);
                }

                // Try fetch the texture using the material name.

                var material = LoadMaterial(contentProvider, materialName);

                if (material == null)
                {

                }

                _patterns[entry.Id] = material == null ? null : new PatternMesh(gameObject, materialName, material?.GetAsUnityMaterial());
            }

            _thickness = new PatternMesh(gameObject, ThicknessTexture, GenerateMaterial(ThicknessTexture).GetAsUnityMaterial());
        }

        private void BuildFloorMeshes()
        {
            // TODO: split pattern meshes by level?
            int width = _patternData.Width;
            int height = _patternData.Height;
            int eHeight = height + 1;

            foreach (PatternMesh pattern in _patterns)
            {
                pattern?.Component.Clear();
            }

            Vector3[] tileVertices = new Vector3[5];
            Vector2[] tileUVs = new Vector2[5];

            Vector3[] thicknessVertices = new Vector3[4];
            Vector2[] thicknessUVs = new Vector2[4];

            var thickComp = _thickness.Component;

            void AddThickness(int from, int to)
            {
                Vector3 fromV = tileVertices[from];
                Vector3 toV = tileVertices[to];

                int baseVert = thickComp.GetVertexIndex();

                thickComp.AddVertex(fromV, new Vector2(0, 1));
                thickComp.AddVertex(toV, new Vector2(1, 1));

                fromV.y -= Thickness;
                toV.y -= Thickness;

                thickComp.AddVertex(toV, new Vector2(1, 0));
                thickComp.AddVertex(fromV, new Vector2(0, 0));

                thickComp.AddIndex(baseVert);
                thickComp.AddIndex(baseVert + 1);
                thickComp.AddIndex(baseVert + 2);

                thickComp.AddIndex(baseVert + 2);
                thickComp.AddIndex(baseVert + 3);
                thickComp.AddIndex(baseVert);
            }

            for (int i = 0; i < _patternData.Depth; i++)
            {
                Vector4<ushort>[] patterns = _patternData.Data[i];
                float[] elevation = _elevationData.Data[i];

                // NOTE: 3D arrays are width then height, rather than height then width.

                int pi = 0;
                int ei = 0;

                float fx = 0;

                for (int x = 0; x < width; x++, fx++)
                {
                    float fy = 0;

                    for (int y = 0; y < height; y++, fy++, ei++, pi++)
                    {
                        ref Vector4<ushort> p = ref patterns[pi];

                        int filledMask = (p.w != 0 ? 1 : 0) | (p.z != 0 ? 2 : 0) | (p.y != 0 ? 4 : 0) | (p.x != 0 ? 8 : 0);

                        if (filledMask != 0)
                        {
                            // Pattern is present.
                            float e0 = elevation[ei];
                            float e1 = elevation[ei + eHeight];
                            float e2 = elevation[ei + eHeight + 1];
                            float e3 = elevation[ei + 1];

                            tileVertices[0] = new Vector3(fx, e0, fy);
                            tileVertices[1] = new Vector3(fx + 1, e1, fy);
                            tileVertices[2] = new Vector3(fx + 1, e2, fy + 1);
                            tileVertices[3] = new Vector3(fx, e3, fy + 1);
                            tileVertices[4] = new Vector3(fx + 0.5f, (e0 + e1 + e2 + e3) / 4, fy + 0.5f);

                            tileUVs[0] = new Vector2(fy, fx);
                            tileUVs[1] = new Vector2(fy, fx + 1);
                            tileUVs[2] = new Vector2(fy + 1, fx + 1);
                            tileUVs[3] = new Vector2(fy + 1, fx);
                            tileUVs[4] = new Vector2(fy + 0.5f, fx + 0.5f);

                            PatternMesh mesh1 = _patterns[p.w];
                            int m1Base = 0;

                            if (mesh1 != null)
                            {
                                var comp = mesh1.Component;
                                m1Base = comp.GetVertexIndex();

                                comp.AddVertices(tileVertices, tileUVs);

                                comp.AddIndex(m1Base + 1);
                                comp.AddIndex(m1Base);
                                comp.AddIndex(m1Base + 4);
                            }

                            PatternMesh mesh2 = _patterns[p.z];
                            int m2Base = 0;

                            if (mesh2 != null)
                            {
                                var comp = mesh2.Component;

                                if (mesh1 == mesh2)
                                {
                                    m2Base = m1Base;
                                }
                                else
                                {
                                    m2Base = comp.GetVertexIndex();
                                    comp.AddVertices(tileVertices, tileUVs);
                                }

                                comp.AddIndex(m2Base + 2);
                                comp.AddIndex(m2Base + 1);
                                comp.AddIndex(m2Base + 4);
                            }

                            PatternMesh mesh3 = _patterns[p.y];
                            int m3Base = 0;

                            if (mesh3 != null)
                            {
                                var comp = mesh3.Component;

                                if (mesh3 == mesh2)
                                {
                                    m3Base = m2Base;
                                }
                                else if (mesh3 == mesh1)
                                {
                                    m3Base = m1Base;
                                }
                                else
                                {
                                    m3Base = comp.GetVertexIndex();
                                    comp.AddVertices(tileVertices, tileUVs);
                                }

                                comp.AddIndex(m3Base + 3);
                                comp.AddIndex(m3Base + 2);
                                comp.AddIndex(m3Base + 4);
                            }

                            PatternMesh mesh4 = _patterns[p.x];

                            if (mesh4 != null)
                            {
                                int m4Base;
                                var comp = mesh4.Component;

                                if (mesh4 == mesh3)
                                {
                                    m4Base = m3Base;
                                }
                                else if (mesh4 == mesh2)
                                {
                                    m4Base = m2Base;
                                }
                                else if (mesh4 == mesh1)
                                {
                                    m4Base = m1Base;
                                }
                                else
                                {
                                    m4Base = comp.GetVertexIndex();
                                    comp.AddVertices(tileVertices, tileUVs);
                                }

                                comp.AddIndex(m4Base);
                                comp.AddIndex(m4Base + 3);
                                comp.AddIndex(m4Base + 4);
                            }

                            if (i > -_baseLevel)
                            {
                                if (filledMask != 15)
                                {
                                    // Missing faces, need to add thickness for diagonals.

                                    for (int tri = 0; tri < 4; tri++)
                                    {
                                        int bit = 1 << tri;

                                        if ((filledMask & bit) != 0)
                                        {
                                            int next = 1 << ((tri + 1) & 3);
                                            int prev = 1 << ((tri + 3) & 3);

                                            if ((filledMask & next) == 0)
                                            {
                                                AddThickness((tri + 1) & 3, 4);
                                            }

                                            if ((filledMask & prev) == 0)
                                            {
                                                AddThickness(4, tri & 3);
                                            }
                                        }
                                    }
                                }

                                // Finally, consider adding thickness for surrounding tiles.

                                if (y > 0 && ((filledMask & 1) != 0) && patterns[pi - 1].y == 0)
                                {
                                    AddThickness(0, 1);
                                }

                                if (x < (width - 1) && ((filledMask & 2) != 0) && patterns[pi + height].x == 0)
                                {
                                    AddThickness(1, 2);
                                }

                                if (y < (height - 1) && ((filledMask & 4) != 0) && patterns[pi + 1].w == 0)
                                {
                                    AddThickness(2, 3);
                                }

                                if (x > 0 && ((filledMask & 8) != 0) && patterns[pi - height].z == 0)
                                {
                                    AddThickness(3, 0);
                                }
                            }
                        }
                    }

                    // Elevation has an extra entry per line.
                    ei++;
                }
            }

            foreach (PatternMesh pattern in _patterns)
            {
                pattern?.Component.Commit();
            }

            _thickness.Component.Commit();
        }
    }
}
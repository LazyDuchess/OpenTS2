using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class LotFloorComponent : AssetReferenceComponent
    {
        private const string ThicknessTexture = "floor-edge";
        private const string FallbackMaterial = "floor-grid";
        public const float Thickness = 0.15f;

        private LotArchitecture _architecture;

        private PatternMeshCollection _patterns;
        private Dictionary<int, HashSet<Collider>> _colliders;

        public LotFloorComponent CreateFromLotArchitecture(LotArchitecture architecture)
        {
            _architecture = architecture;
            _colliders = new Dictionary<int, HashSet<Collider>>();

            LoadPatterns();
            BuildFloorMeshes();

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

            Dictionary<ushort, StringMapEntry> patternMap = _architecture.FloorMap.Map;

            ushort highestId = patternMap.Count == 0 ? (ushort)0 : patternMap.Keys.Max();

            PatternDescriptor[] patterns = new PatternDescriptor[highestId + 2];

            foreach (StringMapEntry entry in patternMap.Values)
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

                patterns[entry.Id + 1] = new PatternDescriptor(
                    materialName,
                    material == null ? null : material?.GetAsUnityMaterial()
                );
            }

            patterns[0] = new PatternDescriptor(
                ThicknessTexture,
                GenerateMaterial(ThicknessTexture).GetAsUnityMaterial()
            );

            _patterns = new PatternMeshCollection(gameObject, patterns, Array.Empty<PatternVariant>(), null, _architecture.FloorPatterns.Depth);
        }

        private void BuildFloorMeshes()
        {
            _3DArrayView<float> elevationData = _architecture.Elevation;
            _3DArrayView<Vector4<ushort>> patternData = _architecture.FloorPatterns;
            int baseFloor = _architecture.BaseFloor;

            int width = patternData.Width;
            int height = patternData.Height;
            int eHeight = height + 1;

            _colliders.Clear();
            _patterns.ClearAll();

            Vector3[] tileVertices = new Vector3[5];
            Vector2[] tileUVs = new Vector2[5];

            LotArchitectureMeshComponent thickComp = null;

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

                thickComp.AddTriangle(baseVert, 0, 1, 2);
                thickComp.AddTriangle(baseVert, 2, 3, 0);
            }

            for (int i = 0; i < patternData.Depth; i++)
            {
                PatternMeshFloor floor = null;

                Vector4<ushort>[] patterns = patternData.Data[i];
                float[] elevation = elevationData.Data[i];

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
                            if (floor == null)
                            {
                                // Lazy init - don't create the floor unless it's actually being used.                                
                                floor = _patterns.GetFloor(i);
                                PatternMesh thickness = floor.Get(0);
                                thickComp = thickness.Component;
                            }
                            if (!_colliders.ContainsKey(i))
                                _colliders.Add(i, new HashSet<Collider>());

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

                            PatternMesh mesh1 = floor.Get(p.w + 1);
                            int m1Base = 0;

                            if (mesh1 != null)
                            {
                                var comp = mesh1.Component;
                                m1Base = comp.GetVertexIndex();

                                comp.AddVertices(tileVertices, tileUVs);
                                comp.AddTriangle(m1Base, 1, 0, 4);

                                _colliders[i].Add(mesh1.Object.GetComponent<MeshCollider>());
                            }

                            PatternMesh mesh2 = floor.Get(p.z + 1);
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

                                comp.AddTriangle(m2Base, 2, 1, 4);
                                _colliders[i].Add(mesh2.Object.GetComponent<MeshCollider>());
                            }

                            PatternMesh mesh3 = floor.Get(p.y + 1);
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

                                comp.AddTriangle(m3Base, 3, 2, 4);
                                _colliders[i].Add(mesh3.Object.GetComponent<MeshCollider>());
                            }

                            PatternMesh mesh4 = floor.Get(p.x + 1);

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

                                comp.AddTriangle(m4Base, 0, 3, 4); 
                                _colliders[i].Add(mesh4.Object.GetComponent<MeshCollider>());
                            }

                            if (i > -baseFloor)
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

            _patterns.CommitAll();
        }

        public void UpdateDisplay(WorldState state)
        {
            _patterns.UpdateDisplay(state, _architecture.BaseFloor);
        }

        internal bool TryGetCollidersByFloor(int floor, out IEnumerable<Collider> Colliders)
        {
            var success = _colliders.TryGetValue(floor, out var value);
            Colliders = value; // cringe out parameter doesn't do implicit casts ;_;
            return success;
        }
    }
}
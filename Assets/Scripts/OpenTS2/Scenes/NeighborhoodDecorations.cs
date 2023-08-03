using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;
using OpenTS2.Components;
using OpenTS2.Engine;
using OpenTS2.Rendering;
using OpenTS2.Diagnostic;

namespace OpenTS2.Scenes
{
    public class NeighborhoodDecorations : AssetReferenceComponent
    {
        [ConsoleProperty("ots2_HoodBatching")]
        private static bool s_enableBatching = true;
        private Dictionary<string, Material> _roadMaterialLookup = new Dictionary<string, Material>();
        private Transform _decorationsParent;
        private Transform _roadsParent;
        private Transform _lotsParent;
        void Start()
        {
            _decorationsParent = new GameObject("Decorations").transform;
            _roadsParent = new GameObject("Roads").transform;
            _lotsParent = new GameObject("Lots").transform;

            _decorationsParent.SetParent(transform);
            _roadsParent.SetParent(transform);
            _lotsParent.SetParent(transform);

            var decorations = NeighborhoodManager.CurrentNeighborhood.Decorations;

            // Render trees.
            RenderDecorationWithModels(decorations.FloraDecorations);
            // Render roads.
            foreach (var road in decorations.RoadDecorations)
            {
                RenderRoad(road);
            }
            // Render bridges.
            RenderBridges(decorations.BridgeDecorations);
            // Render props.
            RenderDecorationWithModels(decorations.PropDecorations);

            // Render lot imposters.
            foreach (var lot in NeighborhoodManager.CurrentNeighborhood.Lots)
            {
                try
                {
                    var imposter = lot.CreateLotImposter();
                    imposter.transform.SetParent(_lotsParent);
                }
                catch (KeyNotFoundException)
                {
                    // Some lots don't have imposters available, that's fine.
                }
            }

            if (s_enableBatching)
            {
                var batchedDeco = Batching.Batch(_decorationsParent, flipFaces: true);
                var batchedLots = Batching.Batch(_lotsParent, flipFaces: true);
                var batchedRoads = Batching.Batch(_roadsParent, flipFaces: false);
            }
        }

        // Clean up the road materials we created. Textures should get garbage collected.
        private void OnDestroy()
        {
            foreach(var mat in _roadMaterialLookup)
            {
                mat.Value.Free();
            }
        }

        private void RenderRoad(RoadDecoration road)
        {
            var roadTextureUnformatted = NeighborhoodManager.CurrentNeighborhood.Terrain.TerrainType.RoadTextureName;
            var roadTextureName = road.GetTextureName(roadTextureUnformatted);
            var roadObject = new GameObject("road", typeof(MeshFilter), typeof(MeshRenderer))
            {
                transform =
                {
                    // Parent to this component.
                    parent = _roadsParent
                }
            };

            var roadMesh = new Mesh();
            roadMesh.SetVertices(road.RoadCorners);
            // Flip triangulation
            // roadMesh.SetTriangles(new[] {/* face1 */ 1, 2, 3, /* face2 */ 1, 3, 0 }, 0);
            roadMesh.SetTriangles(new[] {/* face1 */ 0, 2, 3, /* face2 */ 2, 0, 1 }, 0);

            var connectionFlag = road.ConnectionFlag;
            var uvs = GetUVsForConnectionFlags(connectionFlag);

            roadMesh.SetUVs(0, uvs);
            // TODO - Make this smoother, maybe generate a low res normal map from the terrain height map or calculate normals ourselves.
            roadMesh.RecalculateNormals();
            roadObject.GetComponent<MeshFilter>().sharedMesh = roadMesh;

            if (!_roadMaterialLookup.TryGetValue(roadTextureName, out Material roadMaterial))
            {
                var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(roadTextureName,
                GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

                if (texture == null)
                {
                    Debug.LogWarning($"Failed to find texture for road: {roadTextureName}");
                    return;
                }

                roadMaterial = new Material(Shader.Find("OpenTS2/Road"))
                {
                    mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get())
                };

                roadMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;

                MaterialUtils.SetNeighborhoodParameters(roadMaterial);

                _roadMaterialLookup[roadTextureName] = roadMaterial;

                // Store a reference to the road texture so it doesn't get collected while we use it.
                AddReference(texture);
            }

            roadObject.GetComponent<MeshRenderer>().sharedMaterial = roadMaterial;
        }

        // Returns rotated UVs for a road piece with the given connection flags.
        Vector2[] GetUVsForConnectionFlags(byte connectionFlags)
        {
            var connectionAmount = GetAmountOfConnections(connectionFlags);

            // Connection flag 8 - Connected to piece at -Z
            var uvs = new[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1) };

            // Connection Flag 1 - Connected to piece at -X
            if ((connectionFlags & 1) == 1)
                uvs = new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };

            if (connectionAmount == 1) // Road end
            {
                // Connection Flag 4 - Connected to piece at +X
                if (connectionFlags == 4)
                    uvs = new[] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
                // Connection flag 2 - Connected to piece at +Z
                else if (connectionFlags == 2)
                    uvs = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            }
            else if (connectionAmount == 2) // Road turn
            {
                // Connected to pieces at -Z and +X
                if (connectionFlags == 12)
                    uvs = new[] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
                // Connected to pieces at -X and -Z
                else if (connectionFlags == 9)
                    uvs = new[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1) };
                // Connected to pieces at +Z and +X
                else if (connectionFlags == 6)
                    uvs = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            }
            else if (connectionAmount == 3) // T Junction
            {
                // Going across X, intersecting piece coming from -Z
                if (connectionFlags == 13)
                    uvs = new[] { new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
                // Going across Z, intersecting piece coming from -X
                else if (connectionFlags == 11)
                    uvs = new[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1) };
                // Going across Z, intersecting piece coming from +X
                else if (connectionFlags == 14)
                    uvs = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            }

            return uvs;

            int GetAmountOfConnections(byte connectionFlags)
            {
                var amount = 0;
                if ((connectionFlags & 1) == 1)
                    amount++;
                if ((connectionFlags & 2) == 2)
                    amount++;
                if ((connectionFlags & 4) == 4)
                    amount++;
                if ((connectionFlags & 8) == 8)
                    amount++;
                return amount;
            }
        }

        private void RenderBridges(IEnumerable<BridgeDecoration> bridges)
        {
            foreach (var bridge in bridges)
            {
                // Render the road
                // RenderRoad(bridge.Road)
                var model = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(new ResourceKey(bridge.ResourceName,
                    GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

                var bridgeObject = model.CreateRootGameObject();
                bridgeObject.transform.position = (bridge.Road.Position.Position + bridge.PositionOffset);

                // TODO: put this in a helper MonoBehavior or something.
                var simsRotation = bridgeObject.transform.GetChild(0);
                simsRotation.localRotation = bridge.ModelOrientation;

                // Parent to this component.
                bridgeObject.transform.parent = _decorationsParent;
            }
        }

        private void RenderDecorationWithModels(IEnumerable<DecorationWithObjectId> decorations)
        {
            foreach (var decoration in decorations)
            {
                if (!NeighborhoodManager.NeighborhoodObjects.TryGetValue(decoration.ObjectId, out var resourceName))
                {
                    Debug.Log($"Can't find model for decoration with guid 0x{decoration.ObjectId:X}");
                    return;
                }

                var model = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(new ResourceKey(resourceName,
                    GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

                var decorationObject = model.CreateRootGameObject();
                /*
                if (decorationObject.name.Contains("neighborhood_boulder_house_smoke"))
                {
                    File.WriteAllText("C:\\Users\\ammar\\UnityProjects\\dump.json", model.ResourceCollection.ToString());
                }*/
                decorationObject.transform.position = decoration.Position.Position;

                // TODO: put this in a helper MonoBehavior or something.
                var simsRotation = decorationObject.transform.GetChild(0);
                simsRotation.Rotate(0, 0, decoration.Rotation);

                // Parent to this component.
                decorationObject.transform.parent = _decorationsParent;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes
{
    public class NeighborhoodDecorations : MonoBehaviour
    {
        void Start()
        {
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
                var model = lot.GetLotImposterResource();
                Debug.Log($"lot imposter resource: {model}");
                break;
            }
        }

        private void RenderRoad(RoadDecoration road)
        {
            var roadObject = new GameObject("road", typeof(MeshFilter), typeof(MeshRenderer))
            {
                transform =
                {
                    // Parent to this component.
                    parent = transform
                }
            };

            var roadMesh = new Mesh();
            roadMesh.SetVertices(road.RoadCorners);
            roadMesh.SetTriangles(new []{/* face1 */ 0, 1, 2, /* face2 */  1, 2, 3, /* face3 */ 0, 2, 3}, 0);
            roadMesh.RecalculateNormals();
            roadObject.GetComponent<MeshFilter>().mesh = roadMesh;

            var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(road.TextureName,
                GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

            if (texture == null)
            {
                Debug.LogWarning($"Failed to find texture for road: {road.TextureName}");
                return;
            }

            // TODO: rotate the material appropriately as per road direction.
            var material = new Material(Shader.Find("OpenTS2/StandardMaterial/Opaque"))
            {
                mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get())
            };
            roadObject.GetComponent<MeshRenderer>().material = material;
        }

        private void RenderBridges(IEnumerable<BridgeDecoration> bridges)
        {
            foreach (var bridge in bridges)
            {
                // Render the road
                // RenderRoad(bridge.Road)
                var model = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(new ResourceKey(bridge.ResourceName,
                    GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

                var bridgeObject = model.CreateGameObjectForShape();
                bridgeObject.transform.position = (bridge.Road.Position.Position + bridge.PositionOffset);

                // TODO: put this in a helper MonoBehavior or something.
                var simsRotation = bridgeObject.transform.Find("simsRotations");
                simsRotation.localRotation = bridge.ModelOrientation;

                // Parent to this component.
                bridgeObject.transform.parent = transform;
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

                var decorationObject = model.CreateGameObjectForShape();
                decorationObject.transform.position = decoration.Position.Position;

                // TODO: put this in a helper MonoBehavior or something.
                var simsRotation = decorationObject.transform.Find("simsRotations");
                simsRotation.Rotate(0, 0, decoration.Rotation);

                // Parent to this component.
                decorationObject.transform.parent = transform;
            }
        }
    }
}
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
            var contentProvider = ContentProvider.Get();
            var decorations = NeighborhoodManager.CurrentNeighborhood.Decorations;

            // Render trees.
            RenderDecorationWithModels(decorations.FloraDecorations);
            // Render bridges.
            RenderBridges(decorations.BridgeDecorations);
            // Render props.
            RenderDecorationWithModels(decorations.PropDecorations);
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
using System;
using System.Collections;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Content;
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

            foreach (var flora in decorations.FloraDecorations)
            {
                if (!NeighborhoodManager.NeighborhoodObjects.TryGetValue(flora.ObjectId, out var resourceName))
                {
                    continue;
                }

                var model = contentProvider.GetAsset<ScenegraphResourceAsset>(new ResourceKey(resourceName,
                    GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

                var decorationObject = model.CreateGameObjectForShape();
                decorationObject.transform.position = flora.Position.Position;
                decorationObject.transform.Rotate(0, 0, -flora.Rotation);
            }

            foreach (var prop in decorations.PropDecorations)
            {
                if (!NeighborhoodManager.NeighborhoodObjects.TryGetValue(prop.PropId, out var resourceName))
                {
                    continue;
                }

                var model = contentProvider.GetAsset<ScenegraphResourceAsset>(new ResourceKey(resourceName,
                    GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

                var decorationObject = model.CreateGameObjectForShape();
                decorationObject.transform.position = prop.Position.Position;
                decorationObject.transform.Rotate(0, 0, -prop.Rotation);
            }
        }
    }
}
using System;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphResourceAsset : AbstractAsset
    {
        public ScenegraphResourceCollection ResourceCollection { get; }

        public ScenegraphResourceAsset(ScenegraphResourceCollection resourceCollection) =>
            (ResourceCollection) = (resourceCollection);

        /// <summary>
        /// This renders the full scenegraph graph with the current asset being the root object. This will traverse
        /// the full graph and render any sub-resources with their proper transformations etc.
        ///
        /// The returned game object carries a transform to convert it from sims coordinate space to unity space.
        /// </summary>
        public GameObject CreateRootGameObject()
        {
            var gameObject = CreateGameObject();
            var simsTransform = new GameObject(gameObject.name + "_transform");

            // Apply a transformation to convert from the sims coordinate space to unity.
            simsTransform.transform.Rotate(-90, 0, 0);
            simsTransform.transform.localScale = new Vector3(1, -1, 1);

            gameObject.transform.SetParent(simsTransform.transform, worldPositionStays:false);
            return simsTransform;
        }

        /// <summary>
        /// Same as `CreateRootGameObject` except it doesn't apply the transform from sims to unity space.
        /// </summary>
        public GameObject CreateGameObject()
        {
            var firstResourceNode = ResourceCollection.Blocks.OfType<ResourceNodeBlock>().First();
            var resourceName = firstResourceNode.ResourceName;

            var gameObject = new GameObject(resourceName, typeof(AssetReferenceComponent));
            // Traverse the graph if present and render out any sub-resources.
            try
            {
                if (firstResourceNode.Tree != null)
                {
                    RenderCompositionTree(gameObject, firstResourceNode.Tree);
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Error while traversing graph for {resourceName}");
                Debug.LogException(e);
            }
            return gameObject;
        }

        private void RenderCompositionTree(GameObject parent, CompositionTreeNodeBlock tree)
        {
            foreach (var reference in tree.References)
            {
                switch (reference)
                {
                    case InternalReference internalRef:
                        RenderInternalCompositionTreeChild(parent, internalRef);
                        break;
                    case ExternalReference externalRef:
                        RenderExternalCompositionTreeChild(parent, externalRef);
                        break;
                    case NullReference nullRef:
                        throw new ArgumentException("Got null reference in CompositionTree");
                }
            }

            // Next iterate over all the extension references.
            foreach (var extensionReference in tree.Graph.Extensions)
            {
                if (!(extensionReference is InternalReference internalRef))
                {
                    throw new ArgumentException("Got non-internal reference for Extension in resource node");
                }

                var extension = ResourceCollection.Blocks[internalRef.BlockIndex];
                switch (extension)
                {
                    case BoneDataExtensionBlock boneDataExtensionBlock:
                        // TODO: handle bone data extensions.
                        break;
                    case DataListExtensionBlock extensionBlock:
                        HandleExtension(parent, extensionBlock);
                        break;
                    default:
                        throw new ArgumentException($"Unknown extension block type: {extension}");
                }
            }
        }

        private void HandleExtension(GameObject parent, DataListExtensionBlock extension)
        {
            // This is the only extension we care about right now.
            if (extension.Value.Name != "EffectsList")
            {
                return;
            }

            var effectsList = (DataListValue<DataListValue[]>)extension.Value;
            foreach (var dataListValue in effectsList.Value)
            {
                var effectSlotToName = (DataListValue<string>)dataListValue;

                var slotName = effectSlotToName.Name;
                var effectName = effectSlotToName.Value;
                // The game seriously has effects in the scenegraph that don't exist... ignore those
                if (!EffectsManager.Get().Ready || !EffectsManager.Get().HasEffect(effectName))
                {
                    continue;
                }

                // TODO: these effects are not in the right place, they need to be placed in the right slot. Right now
                // this isn't easy to do as we don't have a mapping of tags -> GameObject. This will be easier once
                // the scenegraph has its own MonoBehavior.
                try
                {
                    var swarmParticleSystem = EffectsManager.Get().CreateEffect(effectName);
                    swarmParticleSystem.transform.SetParent(parent.transform, worldPositionStays: false);
                    swarmParticleSystem.PlayEffect();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unable to render effect {effectName}");
                    Debug.LogException(e);
                }
            }
        }

        private void RenderInternalCompositionTreeChild(GameObject parent, InternalReference reference)
        {
            var block = ResourceCollection.Blocks[reference.BlockIndex];
            switch (block)
            {
                case ShapeRefNodeBlock shapeRef:
                    RenderShapeRefNode(parent, shapeRef);
                    break;
                case TransformNodeBlock transformNode:
                    RenderTransformNode(parent, transformNode);
                    break;
                case ResourceNodeBlock resourceNode:
                    RenderResourceNode(parent, resourceNode);
                    break;
                default:
                    throw new ArgumentException($"Unsupported block type in render composition tree {block}");
            }
        }

        private void RenderExternalCompositionTreeChild(GameObject parent, ExternalReference reference)
        {
            var resourceKey = ResourceCollection.FileLinks[reference.FileLinksIndex];
            switch (resourceKey.TypeID)
            {
                default:
                    throw new ArgumentException($"Unsupported external type in render composition tree: {resourceKey}");
            }
        }

        private void RenderTransformNode(GameObject parent, TransformNodeBlock transformNode)
        {
            var name = "transform";

            var tag = transformNode.CompositionTree.Graph.Tag;
            if (tag != "")
            {
                name = tag;
            }
            var transform = new GameObject(name, typeof(AssetReferenceComponent));
            RenderCompositionTree(transform, transformNode.CompositionTree);

            transform.transform.localRotation = transformNode.Rotation;
            transform.transform.position = transformNode.Transform;

            transform.transform.SetParent(parent.transform, worldPositionStays:false);
        }

        private void RenderResourceNode(GameObject parent, ResourceNodeBlock resource)
        {
            // TODO: handle non-external resources, maybe merge with the `CreateRootGameObject` code.
            var resourceRef = resource.ResourceLocation;
            Debug.Assert(resourceRef is ExternalReference);
            var key = ResourceCollection.FileLinks[((ExternalReference)resourceRef).FileLinksIndex];

            var resourceAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(key);
            if (resourceAsset == null)
            {
                Debug.LogWarning($"Unable to find cResourceNode with key {key} and name {resource.ResourceName}");
                return;
            }
            var resourceObject = resourceAsset.CreateGameObject();
            resourceObject.transform.SetParent(parent.transform, worldPositionStays:false);
        }

        private void RenderShapeRefNode(GameObject parent, ShapeRefNodeBlock shapeRef)
        {
            var shapeTransform = shapeRef.Renderable.Bounded.Transform;

            // TODO: handle multiple shapes here.
            if (shapeRef.Shapes.Length == 0)
            {
                return;
            }
            Debug.Assert(shapeRef.Shapes[0] is ExternalReference);
            var shapeKey = ResourceCollection.FileLinks[((ExternalReference) shapeRef.Shapes[0]).FileLinksIndex];

            if (shapeKey.GroupID == GroupIDs.Local)
            {
                // Use our groupId if the reference has a local group id.
                shapeKey = shapeKey.WithGroupID(GlobalTGI.GroupID);
            }
            var shape = ContentProvider.Get().GetAsset<ScenegraphShapeAsset>(shapeKey);

            // Hold a strong reference to the shape.
            parent.GetComponent<AssetReferenceComponent>().AddReference(shape);

            shape.LoadModelsAndMaterials();
            // Render out each model.
            foreach (var model in shape.Models)
            {
                foreach (var primitive in model.Primitives)
                {
                    // If this group is not listed in the SHPE, we don't render it.
                    if (!shape.Materials.TryGetValue(primitive.Key, out var material))
                    {
                        continue;
                    }

                    GameObject primitiveObject;
                    // Make a SkinnedMeshRender or a regular renderer depending on if we have blend shapes or bones.
                    if (primitive.Value.NeedsSkinnedRenderer)
                    {
                        primitiveObject =
                            new GameObject(primitive.Key, typeof(SkinnedMeshRenderer))
                            {
                                transform =
                                {
                                    rotation = shapeTransform.Rotation,
                                    position = shapeTransform.Transform
                                }
                            };

                        primitiveObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = primitive.Value.Mesh;
                        primitiveObject.GetComponent<SkinnedMeshRenderer>().sharedMaterial =
                            material.GetAsUnityMaterial();
                    }
                    else
                    {
                        primitiveObject =
                            new GameObject(primitive.Key, typeof(MeshFilter), typeof(MeshRenderer))
                            {
                                transform =
                                {
                                    rotation = shapeTransform.Rotation,
                                    position = shapeTransform.Transform
                                }
                            };

                        primitiveObject.GetComponent<MeshFilter>().sharedMesh = primitive.Value.Mesh;
                        primitiveObject.GetComponent<MeshRenderer>().sharedMaterial =
                            material.GetAsUnityMaterial();
                    }

                    primitiveObject.transform.SetParent(parent.transform, worldPositionStays:false);
                }
            }
        }
    }
}
using OpenTS2.Components;
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


        public GameObject CreateGameObjectForShape()
        {
            var resourceNode = ResourceCollection.GetBlockOfType<ResourceNodeBlock>();
            var resourceName = resourceNode.Resource.ResourceName;

            var shapeRef = ResourceCollection.GetBlockOfType<ShapeRefNodeBlock>();
            var shapeTransform = shapeRef.Renderable.Bounded.Transform;

            // TODO: handle multiple shapes here.
            var shapeKey = ResourceCollection.FileLinks[shapeRef.Shapes[0].Index];
            var shape = ContentProvider.Get().GetAsset<ScenegraphShapeAsset>(shapeKey);

            shape.LoadModelsAndMaterials();

            var gameObject = new GameObject(resourceName, typeof(AssetReferenceComponent));

            // Apply a transformation to convert from the sims coordinate space to unity.
            gameObject.transform.Rotate(-90, 0, 0);
            gameObject.transform.localScale = new Vector3(1, -1, 1);

            // Keeps a strong reference to the Shape asset.
            gameObject.GetComponent<AssetReferenceComponent>().AddReference(shape);

            // This is the component that holds rotations from sims space. All rotations from the game such as applying
            // quaternions and angles should be performed on it or components under it.
            var simsRotation = new GameObject("simsRotations");

            // Render out each model.
            foreach (var model in shape.Models)
            {
                foreach (var primitive in model.Primitives)
                {
                    // Create an object for the primitive and parent it to the root game object.
                    var primitiveObject = new GameObject($"{resourceName}_{primitive.Key}", typeof(MeshFilter), typeof(MeshRenderer))
                        {
                            transform =
                            {
                                rotation = shapeTransform.Rotation,
                                position = shapeTransform.Transform
                            }
                        };

                    primitiveObject.GetComponent<MeshFilter>().mesh = primitive.Value;
                    if (shape.Materials.TryGetValue(primitive.Key, out var material))
                    {
                        primitiveObject.GetComponent<MeshRenderer>().material = material.GetAsUnityMaterial();
                    }

                    primitiveObject.transform.SetParent(simsRotation.transform);
                }
            }

            simsRotation.transform.SetParent(gameObject.transform, worldPositionStays:false);
            return gameObject;
        }
    }
}
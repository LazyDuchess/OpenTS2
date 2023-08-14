using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Scenes;
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
            return ScenegraphComponent.CreateRootScenegraph(this);
        }

        /// <summary>
        /// Same as `CreateRootGameObject` except it doesn't apply the transform from sims to unity space.
        /// </summary>
        public GameObject CreateGameObject()
        {
            return ScenegraphComponent.CreateScenegraphComponent(this).gameObject;
        }
    }
}
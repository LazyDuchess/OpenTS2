using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Components
{
    /// <summary>
    /// This component represents a rendered out sims character with their head, hair and body meshes in place under one
    /// scenegraph component.
    /// </summary>
    public class SimCharacterComponent : MonoBehaviour
    {
        public static SimCharacterComponent CreateNakedBaseSim()
        {
            const string nakedBodyResourceName = "amBodyNaked_cres";
            var resource = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(nakedBodyResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            var bodyObject = resource.CreateRootGameObject();
            var scenegraph = bodyObject.GetComponentInChildren<ScenegraphComponent>();

            var gameObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            bodyObject.transform.parent = gameObject.transform;
            return gameObject.GetComponent<SimCharacterComponent>();

            /*
                     resourceName = "amBodyNaked_cres";
        resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
            new ResourceKey(resourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));
        var bodyObject = resource.CreateRootGameObject();

        resourceName = "amHairBald_cres";
        resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
            new ResourceKey(resourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));
        var hairObject = resource.CreateRootGameObject();
             */
        }
    }
}
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
            var nakedBodyAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(nakedBodyResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baldHairResourceName = "amHairBald_cres";
            var baldHairAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baldHairResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baseFaceResourceName = "amFace_cres";
            var baseFaceAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baseFaceResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            var simsObject =
                ScenegraphComponent.CreateRootScenegraph(new[] { nakedBodyAsset, baldHairAsset, baseFaceAsset });
            var scenegraph = simsObject.GetComponentInChildren<ScenegraphComponent>();

            var gameObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            simsObject.transform.parent = gameObject.transform;
            return gameObject.GetComponent<SimCharacterComponent>();
        }
    }
}
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class ScenegraphGMDCTest : MonoBehaviour
{
    private void Start()
    {
        var contentProvider = ContentProvider.Get();

        // Load base game assets.
        contentProvider.AddPackages(
            Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Sims3D"));

        var resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
            new ResourceKey("puFace_cres", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

        Debug.Log($"scenegraphModel: {resource.GlobalTGI}");
        var gameObject = resource.CreateRootGameObject();
        Debug.Log($"gameObject: {gameObject}");
    }

}
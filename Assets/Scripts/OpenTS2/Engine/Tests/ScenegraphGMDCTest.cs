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

        var scenegraphModel = contentProvider.GetAsset<ScenegraphModelAsset>(
            new ResourceKey("ufoCrash_tslocator_gmdc", 0x1C0532FA, TypeIDs.SCENEGRAPH_GMDC));

        Debug.Log($"scenegraphModel: {scenegraphModel.GlobalTGI}");
        Debug.Log($"primitives: {string.Join(" ", scenegraphModel.Primitives.Keys)}");

        RenderPrimitive(scenegraphModel, "ufocrash_body", "ufocrash_body_txmt");
        RenderPrimitive(scenegraphModel, "ufocrash_cabin", "ufocrash_cabin_txmt");
        RenderPrimitive(scenegraphModel, "neighborhood_roundshadow", "neighborhood_roundshadow_txmt");
    }

    private void RenderPrimitive(ScenegraphModelAsset modelAsset, string primitive, string materialDefinition)
    {
        var rendered = new GameObject(primitive, typeof(MeshFilter), typeof(MeshRenderer));
        rendered.transform.Rotate(-90, 0, 0);

        rendered.GetComponent<MeshFilter>().mesh = modelAsset.Primitives[primitive];

        var material = ContentProvider.Get()
            .GetAsset<ScenegraphMaterialDefinitionAsset>(new ResourceKey(materialDefinition, GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_TXMT));
        rendered.GetComponent<MeshRenderer>().material = material.GetAsUnityMaterial();
    }
}
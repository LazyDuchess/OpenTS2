using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes;
using UnityEngine;

public class ScenegraphGMDCTest : MonoBehaviour
{
    private void Start()
    {
        var contentProvider = ContentProvider.Get();

        // Load base game assets.
        contentProvider.AddPackages(
            Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Sims3D"));

        //var resourceName = "vehiclePizza_cres";
        var resourceName = "chairReclinerPuffy_cres";
        var resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
            new ResourceKey(resourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

        Debug.Log($"scenegraphModel: {resource.GlobalTGI}");
        var gameObject = resource.CreateRootGameObject();
        Debug.Log($"gameObject: {gameObject}");

        // For animation testing...
        //AddAnimations(gameObject);
        AddChairAnimations(gameObject);
    }

    private static void AddAnimations(GameObject gameObject)
    {
        var anim = gameObject.GetComponentInChildren<Animation>();
        var scenegraphComponent = gameObject.GetComponentInChildren<ScenegraphComponent>();

        var driveOff = ContentProvider.Get().GetAsset<ScenegraphAnimationAsset>(
            new ResourceKey("o-vehiclePizza-driveOff_anim", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
        var clip = driveOff.CreateClipFromResource(scenegraphComponent.BoneNamesToRelativePaths, scenegraphComponent.BlendNamesToRelativePaths);
        anim.AddClip(clip, "driveOff");

        var drive = ContentProvider.Get().GetAsset<ScenegraphAnimationAsset>(
            new ResourceKey("o-vehiclePizza-drive_anim", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
        clip = drive.CreateClipFromResource(scenegraphComponent.BoneNamesToRelativePaths, scenegraphComponent.BlendNamesToRelativePaths);
        anim.AddClip(clip, "drive");

        var stop = ContentProvider.Get().GetAsset<ScenegraphAnimationAsset>(
            new ResourceKey("o-vehiclePizza-stop_anim", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
        clip = stop.CreateClipFromResource(scenegraphComponent.BoneNamesToRelativePaths, scenegraphComponent.BlendNamesToRelativePaths);
        anim.AddClip(clip, "stop");
    }

    private static void AddChairAnimations(GameObject gameObject)
    {
        var anim = gameObject.GetComponentInChildren<Animation>();
        var scenegraphComponent = gameObject.GetComponentInChildren<ScenegraphComponent>();

        var recline = ContentProvider.Get().GetAsset<ScenegraphAnimationAsset>(
            new ResourceKey("o2a-chairRecliner-recline_anim", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
        var clip = recline.CreateClipFromResource(scenegraphComponent.BoneNamesToRelativePaths, scenegraphComponent.BlendNamesToRelativePaths);
        anim.AddClip(clip, "recline");
    }

}
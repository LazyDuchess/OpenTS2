using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Scenes;
using UnityEngine;


public class EffectsTest : MonoBehaviour
{
    private void Start()
    {
        var contentProvider = ContentProvider.Get();

        // Load base game assets.
        contentProvider.AddPackages(
            Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Sims3D"));

        // Initialize effects manager manually since we aren't using startup controller.
        EffectsManager.Get().Initialize();

        //var effect = EffectsManager.Get().CreateEffectWithUnityTransform("neighborhood_house_smoking");
        var effect = EffectsManager.Get().CreateEffectWithUnityTransform("neighborhood_hanggliders");
        //var effect = EffectsManager.Get().CreateEffectWithUnityTransform("neighborhood_hotairballoon");

        effect.PlayEffect();
    }
}
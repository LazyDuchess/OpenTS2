using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Scenes;
using UnityEngine;


public class EffectsTest : MonoBehaviour
{
    private void Start()
    {
        var contentManager = ContentManager.Instance;

        // Load base game assets.
        contentManager.AddPackages(
            Filesystem.GetPackagesInDirectory(Filesystem.GetPathForProduct(ProductFlags.BaseGame) + "TSData/Res/Sims3D"));

        // Initialize effects manager manually since we aren't using startup controller.
        EffectsManager.Instance.Initialize();

        //var effect = EffectsManager.Get().CreateEffectWithUnityTransform("neighborhood_house_smoking");
        var effect = EffectsManager.Instance.CreateEffectWithUnityTransform("neighborhood_hanggliders");
        //var effect = EffectsManager.Get().CreateEffectWithUnityTransform("neighborhood_hotairballoon");

        effect.PlayEffect();
    }
}
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

        var effect = EffectsManager.Get().CreateEffect("neighborhood_house_smoking");
        //effectsManager.StartEffect("neighborhood_hanggliders");

        effect.PlayEffect();
    }
}
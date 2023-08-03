using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Scenes;
using UnityEngine;


[RequireComponent(typeof(EffectsManager))]
public class EffectsTest : MonoBehaviour
{
    private void Start()
    {
        var contentProvider = ContentProvider.Get();

        // Load base game assets.
        contentProvider.AddPackages(
            Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Sims3D"));

        var effectsManager = GetComponent<EffectsManager>();
        var effect = effectsManager.CreateEffect("neighborhood_house_smoking");
        //effectsManager.StartEffect("neighborhood_hanggliders");

        foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>())
        {
            particles.Play();
        }
    }
}
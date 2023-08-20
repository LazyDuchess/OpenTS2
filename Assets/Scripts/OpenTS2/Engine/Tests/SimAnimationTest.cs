using System;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Files;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class SimAnimationTest : MonoBehaviour
    {
        private void Start()
        {
            var contentProvider = ContentProvider.Get();

            // Load base game assets.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Sims3D"));

            SimCharacterComponent.CreateNakedBaseSim();
        }
    }
}
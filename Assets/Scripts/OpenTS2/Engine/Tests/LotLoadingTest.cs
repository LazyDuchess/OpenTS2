using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class LotLoadingTest : MonoBehaviour
    {
        public string NeighborhoodPrefix = "N001";
        public int LotID = 10;


        private void Start()
        {
            var contentProvider = ContentProvider.Get();
            var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{NeighborhoodPrefix}/Lots");
            var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
            var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);
            var lotPackage = contentProvider.AddPackage(lotFullPath);

            // Load effects.
            EffectsManager.Get().Initialize();
            // Load base game assets.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Sims3D"));

            // Go through each lot object.
            foreach (var entry in lotPackage.Entries)
            {
                if (entry.GlobalTGI.TypeID != TypeIDs.LOT_OBJECT)
                {
                    continue;
                }

                try
                {
                    var lotObject = entry.GetAsset<LotObjectAsset>();
                    var resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
                        new ResourceKey(lotObject.Object.ResourceName + "_cres", GroupIDs.Scenegraph,
                            TypeIDs.SCENEGRAPH_CRES));
                    if (resource == null)
                    {
                        Debug.Log($"Could not find lot object: {lotObject.Object.ResourceName}");
                        continue;
                    }

                    var model = resource.CreateRootGameObject();
                    model.transform.GetChild(0).localPosition = lotObject.Object.Position;
                    model.transform.GetChild(0).localRotation = lotObject.Object.Rotation;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
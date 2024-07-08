using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.SPX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OpenTS2
{
    public class SPXConverter
    {
        [MenuItem("OpenTS2/Experiments/Convert all SPX to WAV")]
        private static void ConvertSPX()
        {
            var baseGameOnly = false;
            var compressed = false;
            if (!EditorUtility.DisplayDialog("SPX to WAV", "This operation will convert ALL SPX resources to WAV. This will take a while and use a lot of resources. Proceed?", "Yes", "No"))
                return;
            baseGameOnly = EditorUtility.DisplayDialog("SPX to WAV", "Which products do you want to convert?", "Base-Game only", "All products");
            compressed = EditorUtility.DisplayDialog("SPX to WAV", "Do you want to compress the resulting packages?", "Yes", "No");
            Core.CoreInitialized = false;
            Core.InitializeCore();
            if (baseGameOnly)
                EPManager.Instance.InstalledProducts = (int)ProductFlags.BaseGame;
            var epManager = EPManager.Instance;
            var products = Filesystem.GetProductDirectories();
            var contentManager = ContentManager.Instance;

            foreach (var product in products)
            {
                var packages = Filesystem.GetPackagesInDirectory(Path.Combine(product, "TSData/Res/Sound"));
                contentManager.AddPackages(packages);
            }

            var audioResources = contentManager.ResourceMap.Where(x => x.Key.TypeID == TypeIDs.AUDIO);
            var speechResources = new List<DBPFEntry>();
            var speechPackages = new HashSet<string>();
            Debug.Log($"Found {audioResources.Count()} Audio Resources.");
            Debug.Log($"Looking for speech resources...");

            new Thread(async () =>
            {
                foreach (var audioResource in audioResources)
                {
                    try
                    {
                        var audioData = audioResource.Value.GetBytes();
                        var magic = Encoding.UTF8.GetString(audioData, 0, 2);
                        if (magic == "SP")
                        {
                            speechResources.Add(audioResource.Value);
                            var packageFileName = Path.GetFileName(audioResource.Value.Package.FilePath);
                            speechPackages.Add(packageFileName);
                        }
                    }
                    catch (Exception) { }
                }

                Debug.Log($"Found {speechResources.Count} SPX audio resources, in {speechPackages.Count} packages.");
                var packagesStr = "Packages:";
                foreach (var package in speechPackages)
                {
                    packagesStr += $"\n{package}";
                }
                Debug.Log(packagesStr);

                GC.Collect();
                foreach (var package in speechPackages)
                {
                    Debug.Log($"Starting work on {package}");
                    var newPackage = new DBPFFile();
                    newPackage.FilePath = Path.Combine("SPX to WAV", package);
                    var entriesToDoForThisPackage = new List<DBPFEntry>();
                    foreach (var spx in speechResources)
                    {
                        try
                        {
                            if (spx.Package == null) continue;
                            if (Path.GetFileName(spx.Package.FilePath) != package) continue;
                            entriesToDoForThisPackage.Add(spx);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    Debug.Log($"Will convert {entriesToDoForThisPackage.Count} Entries");
                    var tasks = new List<Task>();
                    var centry = 0;
                    foreach (var entry in entriesToDoForThisPackage)
                    {
                        centry += 1;
                        var capturedEntry = centry;
                        tasks.Add(Task.Run(() =>
                        {
                            if (capturedEntry % 500 == 0)
                                Debug.Log($"Progress: {capturedEntry}/{entriesToDoForThisPackage.Count}");
                            try
                            {
                                var spxFile = new SPXFile(entry.GetBytes());
                                if (spxFile != null && spxFile.DecompressedData != null && spxFile.DecompressedData.Length > 0)
                                    newPackage.Changes.Set(spxFile.DecompressedData, entry.TGI, compressed);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                        }));
                    }
                    await Task.WhenAll(tasks).ContinueWith((task) =>
                    {
                        try
                        {
                            Debug.Log("Writing to disk...");
                            newPackage.WriteToFile();
                            Debug.Log($"Completed {package}!");
                            GC.Collect();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }, TaskScheduler.Default);
                }
                Debug.Log("All SPX has been converted to WAV! Resulting packages have been written to the SPX to WAV folder.");
            }).Start();
        }
    }
}
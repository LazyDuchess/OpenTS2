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
    public class SpeechEnhancer
    {
        [MenuItem("OpenTS2/Other/Improve Speech Using Tasks")]
        private static void EnhanceSpeechTasks()
        {
            Core.CoreInitialized = false;
            Core.InitializeCore();
            EPManager.Instance.InstalledProducts = (int)ProductFlags.BaseGame;
            var epManager = EPManager.Instance;
            var products = epManager.GetInstalledProducts();
            var contentManager = ContentManager.Instance;

            foreach (var product in products)
            {
                var packages = Filesystem.GetPackagesInDirectory(Path.Combine(Filesystem.GetDataPathForProduct(product), "Res/Sound"));
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
                    newPackage.FilePath = Path.Combine("Enhanced Speech", package);
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
                                    newPackage.Changes.Set(spxFile.DecompressedData, entry.TGI, false);
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
            }).Start();
        }


        [MenuItem("OpenTS2/Other/Improve Speech")]
        private static void EnhanceSpeech()
        {
            Core.CoreInitialized = false;
            Core.InitializeCore();
            EPManager.Instance.InstalledProducts = (int)ProductFlags.BaseGame;
            var epManager = EPManager.Instance;
            var products = epManager.GetInstalledProducts();
            var contentManager = ContentManager.Instance;

            foreach(var product in products)
            {
                var packages = Filesystem.GetPackagesInDirectory(Path.Combine(Filesystem.GetDataPathForProduct(product), "Res/Sound"));
                contentManager.AddPackages(packages);
            }

            var audioResources = contentManager.ResourceMap.Where(x => x.Key.TypeID == TypeIDs.AUDIO);
            var speechResources = new List<DBPFEntry>();
            var speechPackages = new HashSet<string>();
            Debug.Log($"Found {audioResources.Count()} Audio Resources.");
            Debug.Log($"Looking for speech resources...");

            new Thread(() =>
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
                foreach(var package in speechPackages)
                {
                    packagesStr += $"\n{package}";
                }
                Debug.Log(packagesStr);

                GC.Collect();
                foreach(var package in speechPackages)
                {
                    Debug.Log($"Starting work on {package}");
                    var newPackage = new DBPFFile();
                    newPackage.FilePath = Path.Combine("Enhanced Speech", package);
                    var entriesToDoForThisPackage = new List<DBPFEntry>();
                    foreach (var spx in speechResources)
                    {
                        try
                        {
                            if (spx.Package == null) continue;
                            if (Path.GetFileName(spx.Package.FilePath) != package) continue;
                            entriesToDoForThisPackage.Add(spx);
                            //var asset = spx.GetAsset<WAVAudioAsset>();
                            //newPackage.Changes.Set(asset.GetWAVData(), asset.TGI, true);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    Debug.Log($"Will convert {entriesToDoForThisPackage.Count} Entries");
                    var centry = 0;
                    foreach(var entry in entriesToDoForThisPackage)
                    {
                        centry += 1;
                        if (centry % 500 == 0)
                            Debug.Log($"Progress: {centry}/{entriesToDoForThisPackage.Count}");
                            try
                            {
                                var spxFile = new SPXFile(entry.GetBytes());
                                if (spxFile != null && spxFile.DecompressedData != null && spxFile.DecompressedData.Length > 0)
                                    newPackage.Changes.Set(spxFile.DecompressedData, entry.TGI, false);
                            }
                            catch (Exception e) {
                                Debug.LogError(e);
                            }
                    }
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
                }
            }).Start();
        }
    }
}
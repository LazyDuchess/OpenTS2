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
using NAudio;

namespace OpenTS2
{
    public class SPXConverter
    {
        private static async Task ConvertSPXForProduct(ProductFlags product, bool compressed, bool mp3)
        {
            var productDir = Filesystem.GetPathForProduct(product);
            var soundDir = Path.Combine(productDir, "TSData/Res/Sound");
            var packages = Filesystem.GetPackagesInDirectory(soundDir);

            var dbpfFiles = new List<DBPFFile>();

            foreach(var package in packages)
            {
                var dbpf = new DBPFFile(package);
                dbpfFiles.Add(dbpf);
            }

            foreach(var package in dbpfFiles)
            {
                var packageName = Path.GetFileName(package.FilePath);
                var newPackagePath = Path.Combine("SPX2WAV", product.ToString(), $"z{packageName}");
                if (File.Exists(newPackagePath)) continue;
                var newPackage = new DBPFFile();
                newPackage.FilePath = newPackagePath;
                var audioEntries = package.Entries.Where(entry => entry.TGI.TypeID == TypeIDs.AUDIO);
                var speechEntries = new List<DBPFEntry>();
                foreach(var audioEntry in audioEntries)
                {
                    var audioData = audioEntry.GetBytes();
                    var magic = Encoding.UTF8.GetString(audioData, 0, 2);
                    if (magic == "SP")
                    {
                        try
                        {
                            speechEntries.Add(audioEntry);
                        }
                        catch (Exception) { }
                    }
                }
                if (speechEntries.Count <= 0) continue;
                Debug.Log($"Starting work on {packageName}");
                Debug.Log($"Found {speechEntries.Count} SPX audio resources.");
                var tasks = new List<Task>();
                var centry = 0;
                foreach (var entry in speechEntries)
                {
                    centry += 1;
                    var capturedEntry = centry;
                    tasks.Add(Task.Run(() =>
                    {
                        if (capturedEntry % 500 == 0)
                            Debug.Log($"Progress: {capturedEntry}/{speechEntries.Count}");
                        try
                        {
                            var spxFile = new SPXFile(entry.GetBytes());
                            if (spxFile != null && spxFile.DecompressedData != null && spxFile.DecompressedData.Length > 0)
                            {
                                if (mp3)
                                {
                                    var guid = Guid.NewGuid().ToString();
                                    File.WriteAllBytes($"{guid}.wav", spxFile.DecompressedData);
                                    var processAttempts = 0;
                                    System.Diagnostics.Process proc = null;
                                    while (processAttempts < 5)
                                    {
                                        try
                                        {
                                            proc = new System.Diagnostics.Process();
                                            proc.StartInfo.RedirectStandardOutput = true;
                                            proc.StartInfo.UseShellExecute = false;
                                            proc.StartInfo.CreateNoWindow = true;
                                            proc.StartInfo.FileName = "ffmpeg";
                                            proc.StartInfo.Arguments = $"-i {guid}.wav {guid}.mp3";
                                            proc.Start();
                                            proc.WaitForExit();
                                            break;
                                        }
                                        catch(Exception e)
                                        {
                                            Debug.LogError(e);
                                            try
                                            {
                                                if (proc != null && !proc.HasExited)
                                                    proc.Kill();
                                            }
                                            catch (Exception) { }
                                            proc = null;
                                        }
                                        processAttempts++;
                                        try
                                        {
                                            if (proc != null && !proc.HasExited)
                                                proc.Kill();
                                        }
                                        catch (Exception) { }
                                        proc = null;
                                    }
                                    try
                                    {
                                        if (proc != null && !proc.HasExited)
                                            proc.Kill();
                                    }
                                    catch (Exception) { }
                                    proc = null;
                                    if (File.Exists($"{guid}.wav"))
                                    {
                                        Task.Run(() =>
                                        {
                                            File.Delete($"{guid}.wav");
                                        });
                                    }
                                    if (File.Exists($"{guid}.mp3"))
                                    {
                                        var mp3Data = File.ReadAllBytes($"{guid}.mp3");
                                        Task.Run(() =>
                                        {
                                            File.Delete($"{guid}.mp3");
                                        });
                                        newPackage.Changes.Set(mp3Data, entry.TGI, compressed);
                                    }
                                }
                                else
                                {
                                    newPackage.Changes.Set(spxFile.DecompressedData, entry.TGI, compressed);
                                }
                            }
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
                        Debug.Log($"Completed {packageName}!");
                        GC.Collect();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }, TaskScheduler.Default);
            }
        }

        [MenuItem("OpenTS2/Experiments/Convert all SPX to WAV")]
        private static void ConvertSPX()
        {
            var baseGameOnly = false;
            var compressed = false;
            var mp3 = false;
            if (!EditorUtility.DisplayDialog("SPX to WAV", "This operation will convert ALL SPX resources to WAV. This will take a while and use a lot of resources. Proceed?", "Yes", "No"))
                return;
            mp3 = EditorUtility.DisplayDialog("SPX to WAV", "Choose Format", "MP3", "WAV");
            baseGameOnly = EditorUtility.DisplayDialog("SPX to WAV", "Which products do you want to convert?", "Base-Game only", "All products");
            compressed = EditorUtility.DisplayDialog("SPX to WAV", "Do you want to compress the resulting packages?", "Yes", "No");
            Core.CoreInitialized = false;
            Core.InitializeCore();

            new Thread(async () =>
            {
                var productFlags = Enum.GetValues(typeof(ProductFlags));
                var tasks = new List<Task>();
                foreach(ProductFlags productFlag in productFlags)
                {
                    if (baseGameOnly && productFlag != ProductFlags.BaseGame)
                        continue;
                    tasks.Add(Task.Run(async () =>
                    {
                        await ConvertSPXForProduct(productFlag, compressed, mp3);
                    }));
                }
                await Task.WhenAll(tasks).ContinueWith((task) =>
                {
                    Debug.Log("All SPX has been converted to WAV! Resulting packages have been written to the SPX to WAV folder.");
                }, TaskScheduler.Default);
            }).Start();
        }
    }
}
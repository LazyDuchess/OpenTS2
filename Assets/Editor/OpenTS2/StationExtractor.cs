using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.SPX;
using OpenTS2.Files.Formats.XA;
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
    public class StationExtractor
    {
        [MenuItem("OpenTS2/Experiments/Extract Splash Music")]
        private static void ExtractSplash()
        {
            Core.CoreInitialized = false;
            Core.InitializeCore();
            var bgDir = Filesystem.GetPathForProduct(ProductFlags.BaseGame);
            var splashDir = Path.Combine(bgDir, "TSData/Res/Sound/Splash.package");
            var pack = new DBPFFile(splashDir);
            foreach(var splash in pack.Entries)
            {
                var splashData = new XAFile(splash.GetBytes());
                Directory.CreateDirectory("Splash");
                File.WriteAllBytes($"Splash/{splash.GlobalTGI.InstanceID.ToString("X")}.wav", splashData.DecompressedData);
            }
        }
        [MenuItem("OpenTS2/Experiments/Extract Stations")]
        private static void ExtractStations()
        {
            Core.CoreInitialized = false;
            Core.InitializeCore();
            var epManager = EPManager.Instance;
            var products = Filesystem.GetProductDirectories();
            var contentManager = ContentManager.Instance;

            foreach (var product in products)
            {
                var packages = Filesystem.GetPackagesInDirectory(Path.Combine(product, "TSData/Res/Sound"));
                packages.AddRange(Filesystem.GetPackagesInDirectory(Path.Combine(product, "TSData/Res/Text")));
                contentManager.AddPackages(packages);
            }

            AudioManager.Instance.OnFinishedLoading();

            var music = MusicManager.Instance;

            foreach(var cat in music.MusicCategoryByHash)
            {
                var folder = Path.Combine("Stations", cat.Value.Name);
                var playlist = music.GetPlaylist(cat.Value.Name);
                foreach(var song in playlist)
                {
                    var entry = ContentManager.Instance.GetEntry(song.Key);
                    if (entry != null && entry.Package != null)
                    {
                        var data = entry.GetBytes();
                        if (data != null)
                        {
                            Directory.CreateDirectory(folder);
                            var name = song.LocalizedName;
                            if (name.StartsWith("T:"))
                                name = song.Key.InstanceID.ToString("X");
                            File.WriteAllBytes(Path.Combine(folder, name + ".mp3"), data);
                        }
                    }
                }
            }

            Debug.Log("Done extracting stations");
        }
    }
}
/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTS2.Content.Interfaces;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Lua.Disassembly.OpCodes;
using UnityEngine;

namespace OpenTS2.Files
{
    /// <summary>
    /// Handles device filesystem interfacing and path parsing
    /// </summary>
    public static class Filesystem
    {
        [Serializable]
        private class JSONConfig
        {
            public string game_dir;
            public string user_dir;
            public List<string> dlc;
        }

        public static string GameDirectory { get; private set; }
        public static string UserDataDirectory { get; private set; }
        public static List<string> ProductDirectories { get; private set; }

        public static void Initialize()
        {
            ProductDirectories = new List<string>();
        }

        public static void InitializeFromJSON(string jsonPath)
        {
            ProductDirectories = new List<string>();
            var config = JsonUtility.FromJson<JSONConfig>(File.ReadAllText(jsonPath));
            GameDirectory = config.game_dir;
            UserDataDirectory = config.user_dir;
            foreach(var product in config.dlc)
            {
                ProductDirectories.Add(Path.Combine(config.game_dir, product));
            }
        }

        public static List<string> GetProductDirectories()
        {
            var epManager = EPManager.Instance;
            var products = epManager.GetInstalledProducts();
            var result = new List<string>();
            foreach(var product in products)
            {
                result.Add(GetPathForProduct(product));
            }
            result.Add(Environment.CurrentDirectory);
            return result;
        }

        public static string GetPathForProduct(ProductFlags productFlag)
        {
            var index = Array.IndexOf(Enum.GetValues(productFlag.GetType()), productFlag);
            return ProductDirectories[index];
        }

        public static List<string> GetStartupDownloadPackages()
        {
            var downloadsPath = Path.Combine(UserDataDirectory, "Downloads");
            var packages = GetPackagesInDirectory(downloadsPath);
            packages = packages.Where(x => FileUtils.CleanPath(x).ToLowerInvariant().Contains("/startup/")).ToList();
            return packages;
        }

        public static List<string> GetStreamedDownloadPackages()
        {
            var downloadsPath = Path.Combine(UserDataDirectory, "Downloads");
            var packages = GetPackagesInDirectory(downloadsPath);
            packages = packages.Where(x => !FileUtils.CleanPath(x).ToLowerInvariant().Contains("/startup/")).ToList();
            return packages;
        }

        public static List<string> GetUserPackages()
        {
            var packageList = RemoveNeighborhoodAndCCPackagesFromList(GetPackagesInDirectory(UserDataDirectory));
            return packageList;
        }

        public static List<string> GetPackagesForNeighborhood(Neighborhood neighborhood)
        {
            var neighborhoodFolder = Path.GetDirectoryName(neighborhood.PackageFilePath);
            var packages = GetPackagesInDirectory(neighborhoodFolder);
            packages = packages.Where(x => IsNeighborhoodPackage(x)).ToList();
            return packages;
        }

        private static List<string> RemoveNeighborhoodAndCCPackagesFromList(List<string> packages)
        {
            return packages.Where(x => !IsNeighborhoodPackage(x) && !IsDownloadPackage(x)).ToList();
        }

        private static bool IsDownloadPackage(string filename)
        {
            var clean = FileUtils.CleanPath(filename).ToLowerInvariant();
            if (clean.Contains("/downloads/"))
                return true;
            return false;
        }

        private static bool IsNeighborhoodPackage(string filename)
        {
            var clean = FileUtils.CleanPath(filename).ToLowerInvariant();
            if (clean.Contains("/neighborhoods/"))
            {
                if (clean.Contains("/characters/"))
                    return true;
                if (clean.Contains("/lots/"))
                    return true;
                if (clean.Contains("/storytelling/"))
                    return true;
                if (clean.Contains("/thumbnails/"))
                    return true;
            }
            return false;
        }

        public static List<string> GetStartupPackages()
        {
            var startupList = new List<string>();
            var productList = GetProductDirectories();
            foreach(var product in productList)
            {
                var uiPath = Path.Combine(product, "TSData/Res/UI");
                var textPath = Path.Combine(product, "TSData/Res/Text");
                var soundPath = Path.Combine(product, "TSData/Res/Sound");
                startupList.AddRange(GetPackagesInDirectory(uiPath));
                startupList.AddRange(GetPackagesInDirectory(textPath));
                startupList.AddRange(GetPackagesInDirectory(soundPath));
            }
            return startupList;
        }

        public static List<string> GetMainPackages()
        {
            var mainList = new List<string>();
            var productList = GetProductDirectories();
            var curProduct = 0;

            var globalLots = new List<string>();
            var lotTemplates = new List<string>();
            var materials = new List<string>();
            var objects = new List<string>();
            var objectScripts = new List<string>();
            var wants = new List<string>();

            foreach (var product in productList)
            {
                curProduct++;
                var catalogPath = Path.Combine(product, "TSData/Res/Catalog");
                var effectsPath = Path.Combine(product, "TSData/Res/Effects");
                
                var sims3dPath = Path.Combine(product, "TSData/Res/Sims3D");
                var threedPath = Path.Combine(product, "TSData/Res/3D");
                var terrainPath = Path.Combine(product, "TSData/Res/Terrain");
                var overridesPath = Path.Combine(product, "TSData/Res/Overrides");
                var stuffPackPath = Path.Combine(product, "TSData/Res/StuffPack");

                var lightingPath = Path.Combine(product, "TSData/Res/Lighting");

                mainList.AddRange(GetPackagesInDirectory(catalogPath));
                mainList.AddRange(GetPackagesInDirectory(effectsPath));
                
                mainList.AddRange(GetPackagesInDirectory(sims3dPath));
                mainList.AddRange(GetPackagesInDirectory(threedPath));
                mainList.AddRange(GetPackagesInDirectory(terrainPath));
                mainList.AddRange(GetPackagesInDirectory(stuffPackPath));
                mainList.AddRange(GetPackagesInDirectory(overridesPath));

                mainList.AddRange(GetPackagesInDirectory(lightingPath));

                var globalLotsPath = Path.Combine(product, "TSData/Res/GlobalLots");
                if (Directory.Exists(globalLotsPath))
                    globalLots = GetPackagesInDirectory(globalLotsPath);

                var lotTemplatesPath = Path.Combine(product, "TSData/Res/LotTemplates");
                if (Directory.Exists(lotTemplatesPath))
                    lotTemplates = GetPackagesInDirectory(lotTemplatesPath);

                var materialsPath = Path.Combine(product, "TSData/Res/Materials");
                if (Directory.Exists(materialsPath))
                    materials = GetPackagesInDirectory(materialsPath);

                var objectsPath = Path.Combine(product, "TSData/Res/Objects");
                if (Directory.Exists(objectsPath))
                    objects = GetPackagesInDirectory(objectsPath);

                var objectScriptsPath = Path.Combine(product, "TSData/Res/ObjectScripts");
                if (Directory.Exists(objectScriptsPath))
                    objectScripts = GetPackagesInDirectory(objectScriptsPath);

                var wantsPath = Path.Combine(product, "TSData/Res/Wants");
                if (Directory.Exists(wantsPath))
                    wants = GetPackagesInDirectory(wantsPath);
            }
            mainList.AddRange(globalLots);
            mainList.AddRange(lotTemplates);
            mainList.AddRange(materials);
            mainList.AddRange(objects);
            mainList.AddRange(objectScripts);
            mainList.AddRange(wants);
            return mainList;
        }

        public static List<string> GetFilesInPath(string format, string path)
        {
            format = format.ToLowerInvariant();
            if (path.ToLowerInvariant().EndsWith(format) && File.Exists(path))
            {
                var lst = new List<string>
                {
                    path
                };
                return lst;
            }
            if (!Directory.Exists(path))
            {
                return new List<string>();
            }
            string[] allfiles = Directory.GetFiles(path, "*"+format, SearchOption.AllDirectories);
            return allfiles.ToList();
        }

        public static List<string> GetPackagesInDirectory(string directory)
        {
            return GetFilesInPath(".package", directory);
        }

        /// <summary>
        /// Given a file path relative to a product, tries to find the absolute path to the file in the latest installed product where the file is available.
        /// For example, given "TSData/Res/UI/Fonts/FontStyle.ini" in an unmodified installation with all products, this would return the University path, as it's the newest EP that has this file.
        /// </summary>
        /// <param name="filepath">File path relative to TSData</param>
        /// <returns>Absolute file path. Null if file can't be found in any Product.</returns>
        public static string GetLatestFilePath(string filepath)
        {
            var installedProducts = GetProductDirectories();
            for (var i = installedProducts.Count - 1; i >= 0; i--)
            {
                var dataPath = installedProducts[i];
                var absolutePath = Path.Combine(dataPath, filepath);
                if (File.Exists(absolutePath))
                    return absolutePath;
            }
            return null;
        }
    }
}

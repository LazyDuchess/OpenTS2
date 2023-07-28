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

namespace OpenTS2.Files
{
    /// <summary>
    /// Handles device filesystem interfacing and path parsing
    /// </summary>
    public static class Filesystem
    {
        static IPathProvider s_pathProvider;
        static EPManager s_epManager;

        public static IPathProvider PathProvider => s_pathProvider;

        static string UserDataDirectory
        {
            get { return "%UserDataDir%"; }
        }

        static string DataDirectory
        {
            get { return "%DataDirectory%"; }
        }

        static string BinDirectory
        {
            get { return "%BinDirectory%"; }
        }

        public static void Initialize(IPathProvider pathProvider, EPManager EPManager)
        {
            s_pathProvider = pathProvider;
            s_epManager = EPManager;
        }

        public static List<string> GetStartupDownloadPackages()
        {
            var userPath = s_pathProvider.GetUserPath();
            var downloadsPath = Path.Combine(userPath, "Downloads");
            var packages = GetPackagesInDirectory(downloadsPath);
            packages = packages.Where(x => FileUtils.CleanPath(x).ToLowerInvariant().Contains("/startup/")).ToList();
            return packages;
        }

        public static List<string> GetStreamedDownloadPackages()
        {
            var userPath = s_pathProvider.GetUserPath();
            var downloadsPath = Path.Combine(userPath, "Downloads");
            var packages = GetPackagesInDirectory(downloadsPath);
            packages = packages.Where(x => !FileUtils.CleanPath(x).ToLowerInvariant().Contains("/startup/")).ToList();
            return packages;
        }

        public static List<string> GetUserPackages()
        {
            var userPath = s_pathProvider.GetUserPath();
            var packageList = RemoveNeighborhoodAndCCPackagesFromList(GetPackagesInDirectory(userPath));
            return packageList;
        }

        public static List<string> GetPackagesForNeighborhood(Neighborhood neighborhood)
        {
            var neighborhoodFolder = Path.GetDirectoryName(neighborhood.PackageFilePath);
            var packages = GetPackagesInDirectory(neighborhoodFolder);
            packages = packages.Where(x => IsNeighborhoodPackage(x)).ToList();
            return packages;
        }

        static List<string> RemoveNeighborhoodAndCCPackagesFromList(List<string> packages)
        {
            return packages.Where(x => !IsNeighborhoodPackage(x) && !IsDownloadPackage(x)).ToList();
        }

        static bool IsDownloadPackage(string filename)
        {
            var clean = FileUtils.CleanPath(filename).ToLowerInvariant();
            if (clean.Contains("/downloads/"))
                return true;
            return false;
        }

        static bool IsNeighborhoodPackage(string filename)
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
            var productList = s_epManager.GetInstalledProducts();
            foreach(var product in productList)
            {
                var dataPath = s_pathProvider.GetDataPathForProduct(product);
                var uiPath = Path.Combine(dataPath, "Res/UI");
                var textPath = Path.Combine(dataPath, "Res/Text");
                var soundPath = Path.Combine(dataPath, "Res/Sound");
                startupList.AddRange(GetPackagesInDirectory(uiPath));
                startupList.AddRange(GetPackagesInDirectory(textPath));
                startupList.AddRange(GetPackagesInDirectory(soundPath));
            }
            return startupList;
        }

        public static List<string> GetMainPackages()
        {
            var mainList = new List<string>();
            var productList = s_epManager.GetInstalledProducts();
            foreach (var product in productList)
            {
                
                var dataPath = s_pathProvider.GetDataPathForProduct(product);
                var catalogPath = Path.Combine(dataPath, "Res/Catalog");
                var effectsPath = Path.Combine(dataPath, "Res/Effects");
                
                var sims3dPath = Path.Combine(dataPath, "Res/Sims3D");
                var threedPath = Path.Combine(dataPath, "Res/3D");
                var terrainPath = Path.Combine(dataPath, "Res/Terrain");
                var overridesPath = Path.Combine(dataPath, "Res/Overrides");
                var stuffPackPath = Path.Combine(dataPath, "Res/StuffPack");

                var lightingPath = Path.Combine(dataPath, "Res/Lighting");

                mainList.AddRange(GetPackagesInDirectory(catalogPath));
                mainList.AddRange(GetPackagesInDirectory(effectsPath));
                
                mainList.AddRange(GetPackagesInDirectory(sims3dPath));
                mainList.AddRange(GetPackagesInDirectory(threedPath));
                mainList.AddRange(GetPackagesInDirectory(terrainPath));
                mainList.AddRange(GetPackagesInDirectory(stuffPackPath));
                mainList.AddRange(GetPackagesInDirectory(overridesPath));

                mainList.AddRange(GetPackagesInDirectory(lightingPath));

                if (s_epManager.GetLatestProduct() == product)
                {
                    var globalLotsPath = Path.Combine(dataPath, "Res/GlobalLots");
                    mainList.AddRange(GetPackagesInDirectory(globalLotsPath));
                    var lotTemplatesPath = Path.Combine(dataPath, "Res/LotTemplates");
                    mainList.AddRange(GetPackagesInDirectory(lotTemplatesPath));
                    var materialsPath = Path.Combine(dataPath, "Res/Materials");
                    mainList.AddRange(GetPackagesInDirectory(materialsPath));
                    var objectsPath = Path.Combine(dataPath, "Res/Objects");
                    mainList.AddRange(GetPackagesInDirectory(objectsPath));
                    var objectScriptsPath = Path.Combine(dataPath, "Res/ObjectScripts");
                    mainList.AddRange(GetPackagesInDirectory(objectScriptsPath));
                    var wantsPath = Path.Combine(dataPath, "Res/Wants");
                    mainList.AddRange(GetPackagesInDirectory(wantsPath));
                }
            }
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

        public static string GetPathForProduct(ProductFlags product)
        {
            return s_pathProvider.GetPathForProduct(product);
        }

        public static string GetDataPathForProduct(ProductFlags product)
        {
            return s_pathProvider.GetDataPathForProduct(product);
        }

        public static string GetBinPathForProduct(ProductFlags product)
        {
            return s_pathProvider.GetBinPathForProduct(product);
        }

        public static string GetUserPath()
        {
            return s_pathProvider.GetUserPath();
        }

        /// <summary>
        /// Given a file path relative to TSData, tries to find the absolute path to the file in the latest installed product where the file is available.
        /// For example, given "Res/UI/Fonts/FontStyle.ini" in an unmodified installation with all products, this would return the University path, as it's the newest EP that has this file.
        /// </summary>
        /// <param name="filepath">File path relative to TSData</param>
        /// <returns>Absolute file path. Null if file can't be found in any Product.</returns>
        public static string GetLatestFilePath(string filepath)
        {
            var installedProducts = s_epManager.GetInstalledProducts();
            for (var i = installedProducts.Count - 1; i >= 0; i--)
            {
                var dataPath = GetDataPathForProduct(installedProducts[i]);
                var absolutePath = Path.Combine(dataPath, filepath);
                if (File.Exists(absolutePath))
                    return absolutePath;
            }
            return null;
        }

        /// <summary>
        /// Checks if two paths, unparsed or parsed, are equal.
        /// </summary>
        /// <param name="path1">Path to check</param>
        /// <param name="path2">Path to check against</param>
        /// <returns>True if equal, false if not.</returns>
        public static bool PathsEqual(string path1, string path2)
        {
            path1 = GetRealPath(path1).ToLowerInvariant();
            path2 = GetRealPath(path2).ToLowerInvariant();
            return (path1 == path2);
        }
        /// <summary>
        /// Returns short relative path. (Eg. Replaces the game's directory with the %DataDirectory% shorthand)
        /// </summary>
        /// <param name="path">Path to shorten</param>
        /// <returns>Fake short path</returns>
        public static string GetShortPath(string path)
        {
            path = FileUtils.CleanPath(path) + "/";
            path = path.Replace(s_pathProvider.GetDataPathForProduct(s_epManager.GetLatestProduct()), DataDirectory);
            path = path.Replace(s_pathProvider.GetUserPath(), UserDataDirectory);
            path = path.Replace(s_pathProvider.GetBinPathForProduct(s_epManager.GetLatestProduct()), BinDirectory);
            path = FileUtils.CleanPath(path);
            return path;
        }

        /// <summary>
        /// Returns fully parsed path. (Eg. Replaces %DataDirectory% with actual data directory path)
        /// </summary>
        /// <param name="path">Path to parse</param>
        /// <returns>Real path</returns>
        public static string GetRealPath(string path)
        {
            path = path.Replace(DataDirectory, s_pathProvider.GetDataPathForProduct(s_epManager.GetLatestProduct()));
            path = path.Replace(UserDataDirectory, s_pathProvider.GetUserPath());
            path = path.Replace(BinDirectory, s_pathProvider.GetBinPathForProduct(s_epManager.GetLatestProduct()));
            path = FileUtils.CleanPath(path);
            return path;
        }

        /// <summary>
        /// Writes a byte array into a file, creating all necessary directories.
        /// </summary>
        /// <param name="path">Path to output file.</param>
        /// <param name="bytes">Byte array to write.</param>
        public static void Write(string path, byte[] bytes)
        {
            Write(path, bytes, bytes.Length);
        }

        /// <summary>
        /// Writes a byte array into a file, creating all necessary directories.
        /// </summary>
        /// <param name="path">Path to output file.</param>
        /// <param name="bytes">Byte array to write.</param>
        public static void Write(string path, byte[] bytes, int size)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var fStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            fStream.Write(bytes, 0, size);
            fStream.Dispose();
        }
    }
}

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
using OpenTS2.Client;
using OpenTS2.Content.Interfaces;
using OpenTS2.Files;

namespace OpenTS2.Content
{
    /// <summary>
    /// Manages loading game content to the provider.
    /// </summary>
    public static class ContentLoading
    {
        /// <summary>
        /// Synchronously load startup content, bare minimum to display a loading screen.
        /// </summary>
        public static void LoadContentStartup()
        {
            var startupPackages = Filesystem.GetStartupPackages();
            if (Settings.Get().CustomContentEnabled)
                startupPackages.AddRange(Filesystem.GetStartupDownloadPackages());
            ContentProvider.Get().AddPackages(startupPackages);
        }

        /// <summary>
        /// Asynchronously load game content.
        /// </summary>
        /// <returns>Async task.</returns>
        public static async Task LoadGameContentAsync(LoadProgress loadProgress)
        {
            var gamePackages = Filesystem.GetMainPackages();
            gamePackages.AddRange(Filesystem.GetUserPackages());
            if (Settings.Get().CustomContentEnabled)
                gamePackages.AddRange(Filesystem.GetStreamedDownloadPackages());
            await ContentProvider.Get().AddPackagesAsync(gamePackages, loadProgress);
        }

        /// <summary>
        /// Synchronously load game content.
        /// </summary>
        public static void LoadGameContentSync()
        {
            var gamePackages = Filesystem.GetMainPackages();
            gamePackages.AddRange(Filesystem.GetUserPackages());
            if (Settings.Get().CustomContentEnabled)
                gamePackages.AddRange(Filesystem.GetStreamedDownloadPackages());
            ContentProvider.Get().AddPackages(gamePackages);
        }

        public static void LoadNeighborhoodContentSync(Neighborhood neighborhood)
        {
            var neighborhoodPackages = Filesystem.GetPackagesForNeighborhood(neighborhood);
            ContentProvider.Get().AddPackages(neighborhoodPackages);
        }

        public static void UnloadNeighborhoodContentSync()
        {
            var contentProvider = ContentProvider.Get();
            var neighborhoodPackages = Filesystem.GetPackagesForNeighborhood(NeighborhoodManager.CurrentNeighborhood);
            foreach(var packagePath in neighborhoodPackages)
            {
                var package = contentProvider.GetPackageByPath(packagePath);
                if (package != null)
                    contentProvider.RemovePackage(package);
            }
        }
    }
}

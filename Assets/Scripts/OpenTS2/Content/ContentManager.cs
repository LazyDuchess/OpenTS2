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
    /// Manages the game's asset saving/loading/caching and its filesystem.
    /// </summary>
    public class ContentManager
    {
        public ContentProvider Provider;
        public LoadProgress ContentLoadProgress;
        static ContentManager s_instance;
        Settings Settings
        {
            get
            {
                return Settings.Get();
            }
        }

        public ContentCache Cache
        {
            get
            {
                return Provider.Cache;
            }
        }

        public ContentChanges Changes
        {
            get
            {
                return Provider.Changes;
            }
        }

        public static ContentManager Get()
        {
            return s_instance;
        }

        public ContentManager()
        {
            Provider = new ContentProvider();
            ContentLoadProgress = new LoadProgress();
            s_instance = this;
        }

        /// <summary>
        /// Synchronously load startup content, bare minimum to display a loading screen.
        /// </summary>
        public void LoadContentStartup()
        {
            var startupPackages = Filesystem.GetStartupPackages();
            if (Settings.CustomContentEnabled)
                startupPackages.AddRange(Filesystem.GetStartupDownloadPackages());
            Provider.AddPackages(startupPackages);
        }

        /// <summary>
        /// Asynchronously load game content.
        /// </summary>
        /// <returns>Async task.</returns>
        public async Task LoadGameContentAsync()
        {
            var gamePackages = Filesystem.GetMainPackages();
            gamePackages.AddRange(Filesystem.GetUserPackages());
            if (Settings.CustomContentEnabled)
                gamePackages.AddRange(Filesystem.GetStreamedDownloadPackages());
            await Provider.AddPackagesAsync(gamePackages, ContentLoadProgress);
        }

        /// <summary>
        /// Synchronously load game content.
        /// </summary>
        public void LoadGameContentSync()
        {
            var gamePackages = Filesystem.GetMainPackages();
            gamePackages.AddRange(Filesystem.GetUserPackages());
            if (Settings.CustomContentEnabled)
                gamePackages.AddRange(Filesystem.GetStreamedDownloadPackages());
            Provider.AddPackages(gamePackages);
        }
    }
}

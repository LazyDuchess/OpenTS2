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
using OpenTS2.Content.Interfaces;
using UnityEngine;
using System.IO;
using OpenTS2.Content;

namespace OpenTS2.Unity.Content
{
    [System.Serializable]
    public class JSONConfig
    {
        public string game_dir;
        public string user_dir;
        public string lang;
        public List<string> dlc;
    }

    /// <summary>
    /// Provides game paths from a config.json file.
    /// </summary>
    public class JSONPathProvider : IPathProvider
    {
        JSONConfig config;
        public JSONPathProvider()
        {
            var dir = new DirectoryInfo(Application.dataPath).Parent.FullName;
            config = JsonUtility.FromJson<JSONConfig>(File.ReadAllText(Path.Combine(dir, "config.json")));
            ContentManager.FileSystem = new Files.Filesystem(this);
        }
        public List<string> GetGameDataPaths()
        {
            return config.dlc;
        }

        public string GetGameRootPath()
        {
            return config.game_dir;
        }

        public string GetUserPath()
        {
            return config.user_dir;
        }
    }
}

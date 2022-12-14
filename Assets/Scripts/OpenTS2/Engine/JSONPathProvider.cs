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

namespace OpenTS2.Engine
{
    [Serializable]
    public class JSONConfig
    {
        public string game_dir;
        public string user_dir;
        public List<string> dlc;
    }

    /// <summary>
    /// Provides game paths from a config.json file.
    /// </summary>
    public class JSONPathProvider : IPathProvider
    {
        readonly JSONConfig _config;
        public JSONPathProvider()
        {
            var dir = new DirectoryInfo(Application.dataPath).Parent.FullName;
            _config = JsonUtility.FromJson<JSONConfig>(File.ReadAllText(Path.Combine(dir, "config.json")));
        }
        public string GetPathForProduct(ProductFlags productFlag)
        {
            var index = Array.IndexOf(Enum.GetValues(productFlag.GetType()), productFlag);
            return Path.Combine(_config.game_dir, _config.dlc[index]);
        }

        public string GetDataPathForProduct(ProductFlags productFlag)
        {
            return GetPathForProduct(productFlag) + "/TSData";
        }

        public string GetBinPathForProduct(ProductFlags productFlag)
        {
            return GetPathForProduct(productFlag) + "/TSBin";
        }

        public string GetUserPath()
        {
            return _config.user_dir;
        }
    }
}

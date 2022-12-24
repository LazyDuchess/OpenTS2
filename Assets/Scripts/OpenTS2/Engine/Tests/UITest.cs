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
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Content;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;

namespace OpenTS2.Engine.Tests
{
    public class UITest : MonoBehaviour
    {
        public RawImage Image;
        public string PackageToLoad = "%UserDataDirectory%/Neighborhoods/N001/Thumbnails/N001_FamilyThumbnails.package";

        void Start()
        {
            var contentProvider = ContentProvider.Get();
            contentProvider.AddPackage(PackageToLoad);
            var resKey = new ResourceKey(0x00000001, "N001_FamilyThumbnails", 0x8C3CE95A);
            var hasFile = contentProvider.GetEntry(resKey);
            if (hasFile == null)
            {
                Debug.Log("CANT FIND FILE!");
                return;
            }
            var texture = contentProvider.GetAsset<TextureAsset>(resKey);
            Image.texture = texture.Texture;
        }
    }
}

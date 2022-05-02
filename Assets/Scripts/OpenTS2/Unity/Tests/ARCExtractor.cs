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
using OpenTS2.Files.Formats.ARC;
using OpenTS2.Content;
using System.IO;
using OpenTS2.Unity.Content;

namespace OpenTS2.Unity.Tests
{
    
    public class ARCExtractor : MonoBehaviour
    {
        public string ARCPath;
        public string TargetFolder;

        private void Start()
        {
            var codec = new ARCTextureCodec();
            using (var archive = new ARCFile(ARCPath))
            {
                foreach (var element in archive.Entries)
                {
                    try
                    {
                        var imageFile = archive.GetEntryNoHeader(element);
                        var texture2 = codec.Deserialize(imageFile);
                        ContentManager.FileSystem.Write(Path.Combine(TargetFolder, element.FileName + ".png"), ((Texture2D)texture2.engineTexture).EncodeToPNG());
                    }
                    catch (Exception e) { }
                }
            }
        }
    }
}

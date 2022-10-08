/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System.IO;

namespace OpenTS2.Engine.Tests
{
    public class HashTestComponent : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var packageLocation = "C:/Users/Duchess/Documents/EA Games/The Sims™ 2 Ultimate Collection/Downloads/ld_HeightCheater.package";
            Debug.Log("making group from " + Path.GetFileNameWithoutExtension(packageLocation));
            var reftg = new ResourceKey("HeightCheater_cres", 0x1C050000, 0xE519C933);
            var package = new DBPFFile(packageLocation);
            var entry = package.GetItemByTGI(reftg);
            if (entry != null)
                Debug.Log("Found HeightCheater_cres!");
            var objectTGI = new ResourceKey(0x000041A7, "ld_HeightCheater", 0x4F424A44);
            Debug.Log(objectTGI);
            var entry2 = package.GetItemByTGI(objectTGI);
            if (entry2 != null)
                Debug.Log("Found Object!");
            //Debug.Log(reftg.ToString());
        }
    }
}
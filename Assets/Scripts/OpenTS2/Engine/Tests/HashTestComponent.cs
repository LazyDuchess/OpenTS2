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
using OpenTS2.Common.Utils;
using System;
using System.Text;

namespace OpenTS2.Engine.Tests
{
    public class HashTestComponent : MonoBehaviour
    {
        ulong LongHash(byte[] input)
        {
            ulong ret = 0;
            foreach (byte b in input)
            {
                ret <<= 8;
                ret += b;
            }

            return ret;
        }
        // Start is called before the first frame update
        void Start()
        {
            var group = 0x7F8905AA;
            var modelname = "trashcanIndoorValue";
            var name = group.ToString() + modelname;
            // Correct hash for trashcanIndoorValue thumbnail, this is Instance ID below. Instance high is 0x0
            Debug.Log(FileUtils.HighHash(name.Trim().ToLower()).ToString("X8"));

            group = 0x7FA707D7;
            modelname = "chairDiningWeddingOutdoorSash";
            name = group.ToString() + modelname;
            // Correct hash for trashcanIndoorValue thumbnail, this is Instance ID below. Instance high is the group id this time, for some reason.
            Debug.Log(FileUtils.HighHash(name.Trim().ToLower()).ToString("X8"));

            var modelhash = "age4_0_gmnd";
            Debug.Log("0x" + FileUtils.LowHash(modelhash).ToString("X8") + " - 0x" + FileUtils.HighHash(modelhash).ToString("X8"));
        }
    }
}
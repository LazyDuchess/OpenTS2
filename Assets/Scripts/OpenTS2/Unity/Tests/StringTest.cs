/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Content;
using OpenTS2.Common;
using System.Diagnostics;
using OpenTS2.Content.DBPF;

namespace OpenTS2.Unity.Tests
{
    public class StringTest : MonoBehaviour
    {
        public string packagePath = "%UserDataDirectory%Downloads/ld_HeightCheater.package";
        public bool seconds = true;
        public Text text;
        // Start is called before the first frame update
        void Start()
        {
            var stopWatch = new Stopwatch();
            var stopWatchSTR = new Stopwatch();
            var stopWatchSTR2 = new Stopwatch();
            stopWatch.Start();
            ContentManager.Provider.AddPackage(packagePath);
            stopWatch.Stop();
            stopWatchSTR.Start();
            var stringTable = ContentManager.Provider.GetAsset<StringSetAsset>(new TGI(0x0000012D, "ld_heightcheater", 0x53545223));
            stopWatchSTR.Stop();
            stopWatchSTR2.Start();
            ContentManager.Provider.GetAsset<StringSetAsset>(new TGI(0x0000012D, "ld_heightcheater", 0x53545223));
            stopWatchSTR2.Stop();
            text.text = stringTable.GetString(8);
            if (!seconds)
            {
                UnityEngine.Debug.Log("Package loading took " + (stopWatch.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
                UnityEngine.Debug.Log("StringTable Asset loading took " + (stopWatchSTR.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
                UnityEngine.Debug.Log("Cached StringTable Asset loading took " + (stopWatchSTR2.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
            }
            else
            {
                UnityEngine.Debug.Log("Package loading took " + (double)stopWatch.ElapsedMilliseconds / 1000D + " seconds");
                UnityEngine.Debug.Log("StringTable Asset loading took " + (double)stopWatchSTR.ElapsedMilliseconds / 1000D + " seconds");
                UnityEngine.Debug.Log("Cached StringTable Asset loading took " + (double)stopWatchSTR2.ElapsedMilliseconds / 1000D + " seconds");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
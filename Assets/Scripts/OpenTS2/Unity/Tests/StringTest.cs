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
using OpenTS2.Files.Formats.DBPF;

namespace OpenTS2.Unity.Tests
{
    public class StringTest : MonoBehaviour
    {
        public string packagePath = "%UserDataDirectory%Downloads/ld_HeightCheater.package";
        public bool seconds = true;
        public Text text;
        public bool testChanges = false;
        public bool testDeletion = false;
        public bool testSaving = false;

        // Start is called before the first frame update
        void Start()
        {
            var contentProvider = ContentManager.Provider;
            var stopWatch = new Stopwatch();
            var stopWatchSTR = new Stopwatch();
            var stopWatchSTR2 = new Stopwatch();
            stopWatch.Start();
            contentProvider.AddPackage(packagePath);
            stopWatch.Stop();
            stopWatchSTR.Start();
            var stringTable = contentProvider.GetAsset<StringSetAsset>(new ResourceKey(0x0000012D, "ld_heightcheater", TypeIDs.STR));
            stopWatchSTR.Stop();
            if (testChanges)
            {
                stringTable.StringData.strings[1][8] = new StringValue("Test changing stuff", " Oh hi! ");
                stringTable.Compressed = false;
                stringTable.Save();
            }
            stopWatchSTR2.Start();
            stringTable = contentProvider.GetAsset<StringSetAsset>(new ResourceKey(0x0000012D, "ld_heightcheater", TypeIDs.STR));
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

            if (testDeletion)
                stringTable.package.Changes.DeleteAll();

            if (testSaving)
                stringTable.package.WriteToFile();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Engine.Tests
{
    public class StringTest : MonoBehaviour
    {
        public string PackagePath = "%UserDataDirectory%Downloads/ld_HeightCheater.package";
        public bool Seconds = true;
        public Text Text;
        public bool TestChanges = false;
        public bool TestDeletion = false;
        public bool TestSaving = false;
        public bool TestRevert = false;

        // Start is called before the first frame update
        void Start()
        {
            var contentProvider = ContentProvider.Get();
            var stopWatch = new Stopwatch();
            var stopWatchSTR = new Stopwatch();
            var stopWatchSTR2 = new Stopwatch();
            stopWatch.Start();
            contentProvider.AddPackage(PackagePath);
            stopWatch.Stop();
            stopWatchSTR.Start();
            var stringTable = contentProvider.GetAsset<StringSetAsset>(new ResourceKey(0x0000012D, "ld_heightcheater", TypeIDs.STR));
            stopWatchSTR.Stop();
            if (TestChanges)
            {
                stringTable = (StringSetAsset)stringTable.Clone();
                stringTable.StringData.Strings[Languages.USEnglish][8] = new StringValue("Test changing this crap.", " Oh hi! ");
                stringTable.Compressed = false;
                stringTable.Save();
            }

            if (TestDeletion)
                stringTable.Package.Changes.Delete();

            if (TestRevert)
                stringTable.Package.Changes.Clear();

            if (TestSaving)
                stringTable.Package.WriteToFile();

            stopWatchSTR2.Start();
            stringTable = contentProvider.GetAsset<StringSetAsset>(new ResourceKey(0x0000012D, "ld_heightcheater", TypeIDs.STR));
            stopWatchSTR2.Stop();
            
            Text.text = stringTable.GetString(8);
            if (!Seconds)
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
    }
}
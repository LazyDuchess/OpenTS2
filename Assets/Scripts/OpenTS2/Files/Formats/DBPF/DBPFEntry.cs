/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTS2.Common;
using OpenTS2.Content;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// GroupIDs for DBPF archives, defined in sys\\tsosounddata.ini
    /// </summary>
    public enum DBPFGroupID : uint
    {
        Multiplayer = 0x29dd0888,
        Custom = 0x29daa4a6,
        CustomTrks = 0x29d9359d,
        Tracks = 0xa9c6c89a,
        TrackDefs = 0xfdbdbf87,
        tsov2 = 0x69c6c943,
        Samples = 0x9dbdbf89,
        HitLists = 0x9dbdbf74,
        HitListsTemp = 0xc9c6c9b3,
        Stings = 0xddbdbf8c,
        HitLabUI = 0x1d6962cf,
        HitLabTestSamples = 0x1d8a8b4f,
        HitLabTest = 0xbd6e5937,
        EP2 = 0xdde8f5c6,
        EP5Samps = 0x8a6fcc30
    }

    /// <summary>
    /// TypeIDs for DBPF archives, defined in sys\\tsoaudio.ini
    /// </summary>
    public enum DBPFTypeID : uint
    {
        XA = 0x1d07eb4b,
        UTK = 0x1b6b9806,
        WAV = 0xbb7051f5,
        MP3 = 0x3cec2b47,
        TRK = 0x5D73A611,
        HIT = 0x7b1acfcd,
        SoundFX = 0x2026960b,
    }

    /// <summary>
    /// Represents an entry in a DBPF archive.
    /// </summary>
    public class DBPFEntry
    {
        /// <summary>
        /// Global TGI.
        /// </summary>
        public ResourceKey GlobalTGI
        {
            get
            {
                if (Package == null)
                    return TGI;
                return TGI.LocalGroupID(Package.GroupID);
            }
        }

        /// <summary>
        /// Internal TGI, relative to parent package.
        /// </summary>
        public ResourceKey TGI { get; set; } = default(ResourceKey);

        //A 4-byte unsigned integer specifying the offset to the entry's data from the beginning of the archive
        public uint FileOffset;

        //A 4-byte unsigned integer specifying the size of the entry's data
        public virtual uint FileSize { get; set; }

        public DBPFFile Package;
        
        public virtual byte[] GetBytes()
        {
            return Package.GetBytes(this, true);
        }

        public T GetAsset<T>() where T : AbstractAsset
        {
            return GetAsset() as T;
        }

        public virtual AbstractAsset GetAsset()
        {
            return Package.GetAsset(this, true);
        }
    }
}
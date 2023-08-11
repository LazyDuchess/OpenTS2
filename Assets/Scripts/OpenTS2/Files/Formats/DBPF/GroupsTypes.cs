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

namespace OpenTS2.Files.Formats.DBPF
{
    //Could be enums, but I kinda like this better.
    public static class TypeIDs
    {
        /// <summary>
        /// Texture Image in an RCOL wrapper.
        /// </summary>
        public const uint SCENEGRAPH_TXTR = Scenegraph.Block.ImageDataBlock.TYPE_ID;
        public const uint SCENEGRAPH_LIFO = Scenegraph.Block.MipLevelInfoBlock.TYPE_ID;
        public const uint SCENEGRAPH_TXMT = Scenegraph.Block.MaterialDefinitionBlock.TYPE_ID;
        public const uint SCENEGRAPH_GMDC = Scenegraph.Block.GeometryDataContainerBlock.TYPE_ID;
        public const uint SCENEGRAPH_GMND = Scenegraph.Block.GeometryNodeBlock.TYPE_ID;
        public const uint SCENEGRAPH_SHPE = Scenegraph.Block.ShapeBlock.TYPE_ID;
        public const uint SCENEGRAPH_CRES = Scenegraph.Block.ResourceNodeBlock.TYPE_ID;
        public const uint SCENEGRAPH_ANIM = Scenegraph.Block.AnimResourceConstBlock.TYPE_ID;
        public const uint NHOOD_TERRAIN = 0xABCB5DA4;
        // Neighborhood info
        public const uint NHOOD_INFO = 0xAC8A7A2E;
        // Called neighborhood "occupants" in game.
        public const uint NHOOD_DECORATIONS = 0xABD0DC63;
        public const uint NHOOD_OBJECT = 0x6D619378;
        // cTSLotInfo in game / Lot Description in SimsPE.
        public const uint LOT_INFO = 0x0BF999E7;
        public const uint LOT_OBJECT = 0xFA1C39F7;
        public const uint STR = 0x53545223;
        public const uint IMG = 0x856DDBAC;
        public const uint IMG2 = 0x8C3CE95A;
        public const uint EFFECTS = 0xEA5118B0;
        public const uint DIR = 0xE86B1EEF;
        /// <summary>
        /// Can be XA or MP3.
        /// </summary>
        public const uint MP3 = 0x2026960B;
        public const uint OBJD = 0x4F424A44;
        /// dynamiclinklibrary CRC32 hash
        public const uint DLL = 0x7582DEC6;
        public const uint CTSS = 0x43545353;
        public const uint UI = 0x0;
    }
    public static class GroupIDs
    {
        /// <summary>
        /// Local package file group - gets converted to a hash of the package filename.
        /// </summary>
        public const uint Local = 0xFFFFFFFF;
        public const uint DIR = 0xE86B1EEF;
        public const uint Scenegraph = 0x1C0532FA;
        public const uint Effects = 0xEA5118B1;
    }
}

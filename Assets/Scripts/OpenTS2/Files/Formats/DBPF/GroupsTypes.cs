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
        public const uint BASE_LOT_INFO = 0x6C589723;
        public const uint LOT_OBJECT = 0xFA1C39F7;
        public const uint LOT_TERRAIN = 0x6B943B43;
        public const uint LOT_TEXTURES = 0x4B58975B;
        public const uint LOT_FENCEPOST = 0xAB4BA572;
        public const uint LOT_POOL = 0x0C900FDB;
        public const uint LOT_ROOF = 0xAB9406AA;
        public const uint LOT_WALLGRAPH = 0x0A284D0B;
        public const uint LOT_WALLLAYER = 0x8A84D7B0;
        public const uint LOT_STRINGMAP = 0xCAC4FC40;
        public const uint LOT_3ARY = 0x2A51171B;
        public const uint CATALOG_OBJECT = 0xCCA8E925;
        public const uint CATALOG_FENCE = 0x2CB230B8;
        public const uint CATALOG_ROOF = 0xACA8EA06;
        public const uint STR = 0x53545223;
        public const uint IMG = 0x856DDBAC;
        public const uint IMG2 = 0x8C3CE95A;
        public const uint EFFECTS = 0xEA5118B0;
        public const uint DIR = 0xE86B1EEF;
        public const uint AUDIO = 0x2026960B;
        public const uint OBJD = 0x4F424A44;
        /// dynamiclinklibrary CRC32 hash
        public const uint DLL = 0x7582DEC6;
        public const uint CTSS = 0x43545353;
        public const uint UI = 0x0;
        public const uint BHAV = 0x42484156;
        public const uint SEMIGLOBAL = 0x474C4F42;

        public const uint LUA_GLOBAL = 0x9012468A;
        public const uint LUA_LOCAL = 0x9012468B;
        public const uint OBJF = 0x4F424A66;
        public const uint HITLIST = 0x7B1ACFCD;
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
        public const uint Global = 0x7FD46CD0;
    }
}

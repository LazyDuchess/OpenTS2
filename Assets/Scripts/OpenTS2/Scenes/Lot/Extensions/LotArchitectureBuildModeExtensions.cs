using Codice.Utils;
using log4net.Core;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Extensions
{
    //JDrocks450 'Bloaty' 12/9/23 -- Contains all build mode functions with respect to LotArchitecture instances in one unit

    /// <summary>
    /// Extensions for manipulating <see cref="LotArchitecture"/> instances for build mode operations
    /// </summary>
    internal static class LotArchitectureBuildModeExtensions
    {
        /// <summary>
        /// Creates a wall.
        /// <list> <listheader>This function will ensure the following:</listheader>
        /// <item>The Wall is split into 1 unit long segments</item>
        /// <item>The Wall has the wallpaper you provide added to each segment created.</item>
        /// <item>The Wall is a straight line, diagonally, horizontally or vertically aligned</item>
        /// <item>The Wall is of the type provided.</item>
        /// <item>The Wall is added to the <see cref="LotArchitecture"/> instance this is called on</item>
        /// </list>
        /// </summary>
        /// <param name="Architecture"></param>
        /// <param name="From">The point the wall originates</param>
        /// <param name="To">The point the wall is destined to</param>
        /// <param name="Floor">Which floor the wall is on</param>
        /// <param name="Type">The type of wall you wish to create</param>
        /// <param name="PatternFront">The Pattern1 of the wall</param>
        /// <param name="PatternBack">The Pattern2 of the wall</param>
        /// <returns><see langword="null"/> upon total failure; no walls placed.
        /// <see langword="false"/> if some walls were placed. <see langword="true"/> if all wall segments placed.</returns>
        public static bool? CreateWall(this LotArchitecture Architecture, Vector2 From,
            Vector2 To, int Floor, WallType Type = WallType.Normal, ushort PatternFront = ushort.MaxValue, ushort PatternBack = ushort.MaxValue)
        {
            //get the wall's direction vector
            var dirVector = To - From;
            dirVector.Normalize();

            bool diagonal = false;

            if (Math.Abs(dirVector.x) == Math.Abs(dirVector.y))
            {
                dirVector = new Vector2(dirVector.x < 0 ? -1 : 1, dirVector.y < 0 ? -1 : 1);
                diagonal = true;
            }
            if ((Math.Abs(dirVector.x) != 1 || dirVector.y != 0) && (Math.Abs(dirVector.y) != 1 || dirVector.x != 0) &&
                (Math.Abs(dirVector.x) != 1 || Math.Abs(dirVector.y) != 1)) // valid directions for a wall
                return false;

            //subdivide the wall into 1 unit lengths (otherwise wall paints will appear stretchy)
            var wallLength = (double)Math.Abs(Vector2.Distance(From, To));
            if (diagonal) wallLength *= 0.7;

            int successfulWallSegments = 0, totalSegments = (int)wallLength;
            for (int segment = 0; segment < (wallLength); segment++)
            {
                var fooFrom = From + (dirVector * segment);
                var fooTo = From + (dirVector * (segment + 1));
                //Add wall to wallgraph
                if (!Architecture.WallGraphAll.PushWall(fooFrom, fooTo, Floor, out int LayerID)) continue;
                //Add wall layer data
                Architecture.WallLayer.Walls.Add(LayerID, new WallLayerEntry()
                {
                    Id = LayerID,
                    Pattern1 = PatternFront,
                    Pattern2 = PatternBack,
                    WallType = Type
                });
                successfulWallSegments++;
            }
            if (successfulWallSegments <= 0) return null; // total failure
            return successfulWallSegments < totalSegments ? false : true; // somewhat / complete success code
        }
        /// <summary>
        /// Deletes wall segments between the two points provided on the given floor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="floor"></param>
        /// <returns></returns>
        public static bool? DeleteWall(this LotArchitecture Architecture, Vector2 From, Vector2 To, int Floor)
        {
            //get the wall's direction vector
            var dirVector = To - From;
            dirVector.Normalize();

            bool diagonal = false;

            if (Math.Abs(dirVector.x) == Math.Abs(dirVector.y))
            {
                dirVector = new Vector2(dirVector.x < 0 ? -1 : 1, dirVector.y < 0 ? -1 : 1);
                diagonal = true;
            }
            if ((Math.Abs(dirVector.x) != 1 || dirVector.y != 0) && (Math.Abs(dirVector.y) != 1 || dirVector.x != 0) &&
                (Math.Abs(dirVector.x) != 1 || Math.Abs(dirVector.y) != 1)) // valid directions for a wall
                return false;

            //subdivide the wall into 1 unit lengths (otherwise wall paints will appear stretchy)
            var wallLength = (double)Math.Abs(Vector2.Distance(From, To));
            if (diagonal) wallLength *= 0.7;

            int successfulWallSegments = 0, totalSegments = (int)wallLength;
            for (int segment = 0; segment < (wallLength); segment++)
            {
                var fooFrom = From + (dirVector * segment);
                var fooTo = From + (dirVector * (segment + 1));
                //Remove wall from wallgraph
                if (!Architecture.WallGraphAll.RemoveWall(fooFrom, fooTo, Floor, out int LayerID)) continue;
                //Remove wall layer data
                Architecture.WallLayer.Walls.Remove(LayerID);
                successfulWallSegments++;
            }
            if (successfulWallSegments <= 0) return null; // total failure
            return successfulWallSegments < totalSegments ? false : true; // somewhat / complete success code
        }
        /// <summary>
        /// Checks to see if the given pattern name is referenced; if it isn't, will reference it in the <see cref="LotArchitecture.FloorMap"/>
        /// and returns the PatternID of the (potentially newly) referenced pattern
        /// </summary>
        /// <param name="arch"></param>
        /// <param name="PatternName"></param>
        /// <param name="Scope"></param>
        /// <param name="PatternID"></param>
        /// <param name="Existed">true if the pattern was already referenced</param>
        /// <returns></returns>
        public static bool EnsurePatternReferenced(this LotArchitecture arch, string PatternName, 
            LotArchitecture.ArchitectureGameObjectTypes Scope, out ushort PatternID, out bool Existed)
        {
            Existed = false;
            PatternID = 0;

            Dictionary<ushort, StringMapEntry> Map = Scope switch
            {
                LotArchitecture.ArchitectureGameObjectTypes.wall => arch.WallMap.Map,
                LotArchitecture.ArchitectureGameObjectTypes.floor => arch.FloorMap.Map,
                _ => null
            };
            
            if (Map == null) return false;
            if (Map.Count == 0)
                goto create;
            var hits = Map.Where(x => x.Value.Value == PatternName);
            if (!hits.Any())            
                goto create;            
            Existed = true;
            PatternID = hits.FirstOrDefault().Key;
            return true;

        //lord forgive me for using goto
        create:
            PatternID = (ushort)(Map.Count + 1);
            if (Map.Any())
                PatternID = Math.Max(PatternID, (ushort)(Map.Keys.Max() + 1));
            Map.Add(PatternID, new StringMapEntry()
            {
                Id = PatternID,
                Value = PatternName,
                Unknown = 0
            });
            return true;
        }

        public static float PollElevation(this LotArchitecture architecture, Vector2Int Position, int Floor = 0)
        {
            Floor = -architecture.BaseFloor + Floor;

            float[] dataSet = architecture.Elevation.Data[Floor];

            var mPos = Position;
            int index = (mPos.x * architecture.Elevation.Height) + mPos.y;

            return dataSet[index];
        }
    }
}

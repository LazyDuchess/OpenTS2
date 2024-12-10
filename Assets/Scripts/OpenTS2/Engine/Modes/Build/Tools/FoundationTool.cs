using OpenTS2.Content.DBPF;
using OpenTS2.Engine.Modes.Build.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Modes.Build.Tools
{
    /// <summary>
    /// A build tool for creating foundations on the lot
    /// <para>This tool can be boiled down to the <see cref="WallTool"/> and <see cref="FloorTool"/> put together</para>
    /// </summary>
    internal class FoundationTool : AbstractRegionSelectBuildTool
    {
        const float FoundationHeight = 1;
        const string FoundationWallpaper = "foundationbrick";
        private const string FoundationFlooring = "wood_wide_planks";

        /// <summary>
        /// Sets up a new Foundation Tool with the specified walls and floors
        /// <para>By default this is The Sims 2 default values Brick walls and Wood floors</para>
        /// </summary>
        /// <param name="Server"></param>
        /// <param name="Wallpaper"></param>
        /// <param name="Flooring"></param>
        public FoundationTool(BuildModeServer Server, 
            string Wallpaper = FoundationWallpaper, 
            string Flooring = FoundationFlooring) : base(Server)
        {
            WallpaperPattern = Wallpaper;
            FlooringPattern = Flooring;
        }

        public override string ToolName => "Foundation Tool";
        public override BuildTools ToolType => BuildTools.Foundation;

        //**PATTERN 
        /// <summary>
        /// The pattern to paint the walls when creating a Foundation
        /// </summary>
        public string WallpaperPattern { get; set; }
        /// <summary>
        /// The pattern to paint the floors when creating a Foundation
        /// </summary>
        public string FlooringPattern { get; set; }
        //***

        protected override MultilevelBehavior MultiLevelBehavior { get; set; } = MultilevelBehavior.Constrain;
        

        protected override void DoAction(bool Undo = false)
        {
            if (DeleteMode) Undo = !Undo;
            if (ToolDragFloor > 1)
            {
                OnToolCancel("Foundation can only be placed on terrain.");
                return;
            }

            int currFloor = 0;
            int nextFloor = currFloor + 1;

            float elevation = BuildModeServer.PollElevation(ToolDragOrigin, ToolDragFloor);
            if (ToolDragFloor == 0)
                elevation += FoundationHeight;
            //This levels the Elevation map where the floor is on this foundation.
            //This makes the walls stubby and the floor appear seamless. This also uses the LevelBeneathMe feature of the function.
            //It will ensure all terrain beneath the foundation is less than the elevation -.1f.
            BuildModeServer.LevelRegion(ToolDragOrigin, ToolDragDestination, elevation, nextFloor, true, -.1f);
            if (!Undo)
            {
                BuildModeServer.CreateWalls(ToolDragOrigin, ToolDragDestination, currFloor,
                    Content.DBPF.WallType.Foundation, BuildModeServer.WallCreationModes.Room,
                    WallpaperPattern, WallpaperPattern);
                BuildModeServer.CreateFloors(ToolDragOrigin, ToolDragDestination, FlooringPattern, nextFloor, false);
            }
            else
            {
                BuildModeServer.DeleteWalls(ToolDragOrigin, ToolDragDestination, currFloor, BuildModeServer.WallCreationModes.Room);
                BuildModeServer.DeleteFloors(ToolDragOrigin, ToolDragDestination, nextFloor);
            }
        }

        protected override void DoHoverAction(bool Undo = false)
        {
            ;
        }
    }
}

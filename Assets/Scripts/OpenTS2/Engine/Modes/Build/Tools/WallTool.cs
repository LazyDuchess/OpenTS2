using OpenTS2.Content.DBPF;
using OpenTS2.Engine.Modes.Build.Tools;
using OpenTS2.Scenes.Lot;
using OpenTS2.Scenes.Lot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static OpenTS2.Engine.Modes.Build.BuildModeServer;

namespace OpenTS2.Engine.Modes.Build.Tools
{    
    /// <summary>
    /// Handles the functionality for the Wall Tool interacting with the lot
    /// </summary>
    internal class WallTool : AbstractRegionSelectBuildTool
    {
        // Wall cursor tool
        WallCreationModes wallCreateMode = WallCreationModes.Single;
        bool deletingWalls => DeleteMode;

        //private
        BuildModeServer buildModeServer => BuildModeServer;

        /// <summary>
        /// Creates a new <see cref="WallTool"/> on the specified lot
        /// </summary>
        /// <param name="loadedLot"></param>
        /// <param name="architecture"></param>
        public WallTool(BuildModeServer Server) : base(Server) { }     

        public override string ToolName => "Wall Tool";
        /// <summary>
        /// Updates <see cref="wallCreateMode"/> depending on the player's current input
        /// </summary>
        protected override void CheckMode()
        {
            //creation mode
            WallCreationModes mode = WallCreationModes.Single;
            if (Input.GetKey(KeyCode.LeftShift)) mode = WallCreationModes.Room;
            wallCreateMode = mode;

            //check delete mode
            base.CheckMode();
        }

        /// <summary>
        /// Creates/Deletes the walls in the selected space
        /// </summary>
        protected override void DoAction(bool Undoing = false)
        {
            //NEEDS REFACTOR REALLY BAD
            //Get layer IDs from the created walls when making a facade
            //that way, we only delete walls SUCCESSFULLY MADE and also it won't be so buggy
            //and performance will be (slightly) better
            //please do this soon jeremy >.<

            bool actionSelected = !deletingWalls;
            Vector2Int destination = toolDragEnd;
            if (deletingWalls) destination = toolLastActionDragEnd;
            if (Undoing)
            {
                if(destination != toolLastActionDragEnd)
                    destination = toolLastActionDragEnd;
                else destination = toolDragEnd;
                actionSelected = !actionSelected;
            }
            if (toolDragStart - destination == new Vector2Int(0, 0)) return; // zero area 
            if (actionSelected)
            {
                float startElevation = buildModeServer.PollElevation(toolDragStart, toolDragFloor);
                //level upper elevation to ensure the wall is the correct size
                foreach (var segment in GetWallPoints(toolDragStart, destination, wallCreateMode))
                {
                    //level terrain beneath the wall
                    //buildModeServer.SetElevationRelative(startElevation, toolDragFloor, segment.A, segment.B);
                    //level above the wall to ensure the height is accurate
                    buildModeServer.SetElevationRelative(LotWallComponent.WallHeight, toolDragFloor + 1, segment.A, segment.B);
                }            
                buildModeServer.CreateWalls(toolDragStart, destination, toolDragFloor, WallType.Normal, wallCreateMode);
            }
            else buildModeServer.DeleteWalls(toolDragStart, destination, toolDragFloor, wallCreateMode);
        }

        protected override void DoHoverAction(bool Undo = false) => DoAction(Undo);
    }
}

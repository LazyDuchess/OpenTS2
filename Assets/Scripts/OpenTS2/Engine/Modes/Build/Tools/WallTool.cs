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
        /// <summary>
        /// This is used when dragging an area of walls. 
        /// <para/>The walls created as a facade have their IDs stored here.
        /// When the action is cancelled or the facade changes, these IDs
        /// will be used to delete the unneeded walls.
        /// </summary>
        HashSet<int> facadeWallIDs = new HashSet<int>();
        BuildModeServer buildModeServer => BuildModeServer;
        bool Hovering = false;

        /// <summary>
        /// Creates a new <see cref="WallTool"/> on the specified lot
        /// </summary>
        /// <param name="loadedLot"></param>
        /// <param name="architecture"></param>
        public WallTool(BuildModeServer Server) : base(Server) { }     

        public override string ToolName => "Wall Tool";
        public override BuildTools ToolType => BuildTools.Wall;

        /// <summary>
        /// Updates <see cref="wallCreateMode"/> depending on the player's current input
        /// </summary>
        protected override void CheckDeleteMode()
        {
            //creation mode
            WallCreationModes mode = WallCreationModes.Single;
            if (Input.GetKey(KeyCode.LeftShift)) mode = WallCreationModes.Room;
            wallCreateMode = mode;

            //check delete mode
            base.CheckDeleteMode();
        }

        /// <summary>
        /// Creates/Deletes the walls in the selected space
        /// </summary>
        protected override void DoAction(bool Undoing = false)
        {
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
            if (toolDragStart - destination == new Vector2Int(0, 0))
            {
                if (Hovering && facadeWallIDs.Count > 0)
                {
                    buildModeServer.DeleteAllWalls(facadeWallIDs.ToArray());
                    facadeWallIDs.Clear();
                }
                return; // zero area 
            }
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
                var createdWallIDs = buildModeServer.CreateWalls(toolDragStart, destination, toolDragFloor, WallType.Normal, wallCreateMode);
                if (Hovering)
                {
                    if (createdWallIDs != null)
                        Array.ForEach(createdWallIDs, (int item) => facadeWallIDs.Add(item));
                }
                else if (facadeWallIDs.Any())
                {
                    facadeWallIDs.Clear();
                    buildModeServer.SignalRoomsInvalidate();
                }
            }
            else
            {
                if (Hovering && facadeWallIDs.Count > 0)
                {
                    buildModeServer.DeleteAllWalls(facadeWallIDs.ToArray());
                    facadeWallIDs.Clear();
                }
                buildModeServer.DeleteWalls(toolDragStart, destination, ToolDragFloor, wallCreateMode);                
            }
        }

        protected override void DoHoverAction(bool Undo = false)
        {
            Hovering = true;
            DoAction(Undo);
            Hovering = false;
        }
    }
}

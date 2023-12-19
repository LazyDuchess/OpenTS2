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
        public FoundationTool(BuildModeServer Server) : base(Server)
        {
        }

        public override string ToolName => "Foundation Tool";
        protected override MultilevelBehavior MultiLevelBehavior { get; set; } = MultilevelBehavior.Constrain;

        protected override void DoAction(bool Undo = false)
        {
            if (DeleteMode) Undo = !Undo;
            float change = 1;
            if (ToolDragFloor > 1)
            {
                OnToolCancel("Foundation can only be placed on terrain.");
                return;
            }
            else if (ToolDragFloor == 1) change = 0;

            int currFloor = 0;
            int nextFloor = currFloor + 1;

            float elevation = BuildModeServer.PollElevation(ToolDragOrigin, ToolDragFloor) + change;
            BuildModeServer.LevelRegion(ToolDragOrigin, ToolDragDestination, elevation, nextFloor);
            if (!Undo)
            {
                BuildModeServer.CreateWalls(ToolDragOrigin, ToolDragDestination, currFloor,
                    Content.DBPF.WallType.Foundation, BuildModeServer.WallCreationModes.Room,
                    "foundationbrick", "foundationbrick");
                BuildModeServer.CreateFloors(ToolDragOrigin, ToolDragDestination, "wood_wide_planks", nextFloor, false);
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

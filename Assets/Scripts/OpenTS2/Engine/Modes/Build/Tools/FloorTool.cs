namespace OpenTS2.Engine.Modes.Build.Tools
{
    /// <summary>
    /// Build tools that allow the user to make selections using the catalog in Build Mode
    /// </summary>
    internal interface ICatalogSelectableBuildTool
    {

    }
    /// <summary>
    /// Build tool that takes patterns from Build Mode (walls and floors)
    /// </summary>
    internal interface IPatternSelectableBuildTool
    {
        /// <summary>
        /// Dictates whether the tool should be using the <see cref="PatternID"/> or <see cref="PatternName"/> property
        /// </summary>
        bool IDPainting { get; }

        /// <summary>
        /// The name of the pattern's material
        /// </summary>
        string PatternName { get; }
        /// <summary>
        /// The ID of the pattern in the target component map.
        /// <para/>Walls and Floors (for example) both have individual WallMaps and FloorMaps
        /// independent of one another and this mode should generally not be used in favor of <see cref="PatternName"/>.
        /// </summary>
        ushort PatternID { get; }

        /// <summary>
        /// Sets the pattern the floor tool is currently painting
        /// </summary>
        void SetPaintPattern(string PatternName);
        /// <summary>
        /// Sets the pattern the floor tool is currently painting
        /// </summary>
        void SetPaintPatternID(ushort PatternID);
    }

    /// <summary>
    /// This Build Tool allows the User to drag out areas of flooring.
    /// <para/>It invokes the <see cref="BuildModeServer"/> for all lot modifications
    /// </summary>
    internal class FloorTool : AbstractRegionSelectBuildTool, IPatternSelectableBuildTool
    {
        public override string ToolName => "Floor Tool";

        public bool IDPainting { get; private set; }
        public string PatternName { get; private set; }
        public ushort PatternID { get; private set; }

        public FloorTool(BuildModeServer Server) : base(Server) { }

        public void SetPaintPattern(string PatternName)
        {
            IDPainting = false;
            this.PatternName = PatternName;
        }
        public void SetPaintPatternID(ushort PatternID)
        {
            IDPainting = true;
            this.PatternID = PatternID;
        }

        protected override void DoAction(bool Undo = false)
        {
            if (DeleteMode)
                BuildModeServer.DeleteFloors(ToolDragOrigin, ToolDragDestination, ToolDragFloor);
            else
            {
                if (IDPainting)
                    BuildModeServer.CreateFloors(ToolDragOrigin, ToolDragDestination, PatternID, ToolDragFloor);
                else BuildModeServer.CreateFloors(ToolDragOrigin, ToolDragDestination, PatternName, ToolDragFloor);
            }
        }

        protected override void DoHoverAction(bool Undo = false)
        {
            ;
        }
    }
}

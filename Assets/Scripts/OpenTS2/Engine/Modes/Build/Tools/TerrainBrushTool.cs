using UnityEngine;
using static OpenTS2.Engine.Modes.Build.BuildModeServer;

namespace OpenTS2.Engine.Modes.Build.Tools
{
    /// <summary>
    /// Handles the functionality for the Wall Tool interacting with the lot
    /// </summary>
    internal class TerrainBrushTool : AbstractBuildTool
    {         
        /// <summary>
        /// Radii of terrain brush sizes
        /// </summary>
        public enum TerrainBrushSizes
        {
            Small = 0, // since this tool is only 1 point, it's not a tile wide
            Medium = 2,
            Large  = 4
        }

        //private
        BuildModeServer buildModeServer;
        ushort dryWallDesignPattern;
        private GameObject rootWallCursor;
        private Transform _wallCursorTransform;

        /// <summary>
        /// The current brush mode the tool is using
        /// </summary>
        public BuildModeServer.TerrainModificationModes CurrentBrushMode { get; set; }
        /// <summary>
        /// The current stroke thickness of the brush
        /// </summary>
        public int BrushSize { get; set; } = (int)TerrainBrushSizes.Large;
        public float BrushStrength { get; set; } = 1f;

        /// <summary>
        /// Creates a new <see cref="TerrainBrushTool"/> on the specified lot
        /// </summary>
        /// <param name="loadedLot"></param>
        /// <param name="architecture"></param>
        public TerrainBrushTool(BuildModeServer Server)
        { 
            buildModeServer = Server;
            Init();
        }

        void Init()
        {
            //get the default Wand
            rootWallCursor = GameObject.Find("Wand");
            _wallCursorTransform = rootWallCursor.transform;
        }

        public override string ToolName => "Terrain Tool";
        public override BuildTools ToolType => BuildTools.TerrainBrush;

        protected override void OnActiveChanged(bool NewValue)
        {
            rootWallCursor.SetActive(NewValue);
            CurrentBrushMode = TerrainModificationModes.Raise;
        }

        public override void OnToolCancel(string Reason)
        {
            IsHolding = false; // drop tool

            Debug.Log($"{ToolName} cancelled. Reason: {Reason ?? "null"}");
        }

        public override void OnToolFinalize(BuildToolContext Context)
        {            
            IsHolding = false; // finish brush stroke           

            base.OnToolFinalize(Context);
        }

        public override void OnToolStart(BuildToolContext Context)
        {
            if (IsHolding) return; // huh? weird edge case here
            IsHolding = true; // brush stroke start

            CommitToolStroke(Context.GridPosition); // makes single clicks (only lasting a frame) possible
        }

        public override void OnToolUpdate(BuildToolContext Context)
        {
            base.OnToolUpdate(Context);

            if (!IsActive) return;
            _wallCursorTransform.position = Context.WandPosition;
            if (!IsHolding) return;            

            //User let go of the Wand, signal finalize event
            if (!Input.GetMouseButton(0))
            { // finalize
                OnToolFinalize(Context);
                return;
            }
            //user is still painting
            CommitToolStroke(Context.GridPosition);
        }

        void CommitToolStroke(Vector2Int position)
        {
            buildModeServer.ModifyTerrain(position, BrushSize, BrushStrength * Time.deltaTime, CurrentBrushMode);
        }
    }
}

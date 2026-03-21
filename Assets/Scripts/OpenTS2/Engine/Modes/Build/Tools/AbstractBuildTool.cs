using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Modes.Build.Tools
{
    /// <summary>
    /// Context passed a parameter for functions on a <see cref="AbstractBuildTool"/> instance
    /// </summary>
    internal class BuildToolContext
    {
        /// <summary>
        /// The position of the 3D mouse cursor in world space
        /// </summary>
        public Vector3 Cursor3DWorldPosition { get; set; }
        /// <summary>
        /// The position of the wand snapped to grid coordinates
        /// </summary>
        public Vector3 WandPosition { get; set; }
        /// <summary>
        /// The position of the wand in the 2D lot grid
        /// </summary>
        public Vector2Int GridPosition { get; set; }
        /// <summary>
        /// The quadrant the 3D mouse cursor is in
        /// <code>Chart Quadrants: 1 = top left, 2 = top right, 3 = bottom right, 4 = bottom left</code>
        /// </summary>
        public int GridTileCursorQuadrant { get; set; }
        public int CursorFloor { get; internal set; }
    }

    /// <summary>
    /// A build tool with basic Pick-Up and Put-Down functionality
    /// </summary>
    internal abstract class AbstractBuildTool
    {        
        public delegate void OnFinalizeToolHandler(AbstractBuildTool sender);
        /// <summary>
        /// Called when the user releases the tool and the changes will be submitted.
        /// </summary>
        public event OnFinalizeToolHandler OnFinalizeTool;

        /// <summary>
        /// The name of this tool
        /// </summary>
        public abstract string ToolName { get; }
        /// <summary>
        /// The current type of tool this instance is acting as
        /// </summary>
        public abstract BuildTools ToolType { get; }
        /// <summary>
        /// True when the tool is actively being used by the user. (A wall is being created, for instance)
        /// </summary>
        public bool IsHolding { get; protected set; }
        /// <summary>
        /// True when the tool is not being actively used but is ready for user input
        /// </summary>
        public bool IsActive { get; private set; } = false;
        /// <summary>
        /// The position within the lot boundaries of the tool at the current time
        /// </summary>
        public Vector3 ToolLotPosition { get; protected set; }
        /// <summary>
        /// The position on the grid that the tool is currently nearest to
        /// </summary>
        public Vector2 ToolGridPosition { get; protected set; }
        /// <summary>
        /// Sets the <see cref="IsActive"/> property
        /// </summary>
        /// <param name="Activated"></param>
        public void SetActive(bool Activated)
        {
            IsActive = Activated;
            OnActiveChanged(Activated);
        }
        protected abstract void OnActiveChanged(bool NewValue);
        /// <summary>
        /// Called once per frame for the tool to process changes between frames
        /// </summary>
        /// <param name="Context"></param>
        public virtual void OnToolUpdate(BuildToolContext Context)
        {
            ToolLotPosition = Context.WandPosition;
            ToolGridPosition = Context.GridPosition;
        }
        /// <summary>
        /// Called when a tool begins to be used by the user
        /// </summary>
        /// <param name="Context"></param>
        public abstract void OnToolStart(BuildToolContext Context);   
        /// <summary>
        /// Called when a tool is cancelled.
        /// <para/>Note: Calling this at any time will immediately cancel the current tool action, you can give a reason to add clarity
        /// to why this was called.
        /// </summary>
        /// <param name="Reason"></param>
        public abstract void OnToolCancel(string Reason = null);
        /// <summary>
        /// Called when the user commits to an action. (Left click)
        /// </summary>
        /// <param name="Context"></param>
        public virtual void OnToolFinalize(BuildToolContext Context) => OnFinalizeTool?.Invoke(this);
    }

    /// <summary>
    /// Represents shared behavior for tools in Build Mode where the user can click and drag an area to manipulate the lot
    /// </summary>
    internal abstract class AbstractRegionSelectBuildTool : AbstractBuildTool
    {
        //protected fields
        /// <summary>
        /// Defines behaviors for how the cursor should react to moving between levels
        /// </summary>
        protected enum MultilevelBehavior
        {            
            /// <summary>
            /// If the cursor leaves the level it started dragging, cancel the action immediately.
            /// </summary>
            CancelAction,
            /// <summary>
            /// If the cursor is not at the same level as it started when the action is finished, cancel the action.
            /// <para/>Unlike <see cref="MultilevelBehavior.CancelAction"/>, the action won't be cancelled immediately.
            /// </summary>
            Deny,
            /// <summary>
            /// Allows the cursor to freely move between levels and will not intervene.
            /// </summary>
            Allow,
            /// <summary>
            /// Keeps the level the same as when the tool started dragging so the level is retained even when the cursor moves between levels
            /// </summary>
            Constrain,
        }

        protected Vector2Int toolDragStart;
        protected Vector2Int toolDragEnd;
        protected Vector2Int toolLastActionDragEnd;
        protected Vector3 toolDragStartWorldPos;
        protected int toolDragFloor;

        //public properties 
        /// <summary>
        /// Where the user drag gesture initiated, in lot grid coordinates
        /// </summary>
        public Vector2Int ToolDragOrigin => toolDragStart;
        /// <summary>
        /// Where the user drag gesture ended, in lot grid coordinates
        /// </summary>
        public Vector2Int ToolDragDestination => toolDragEnd;
        /// <summary>
        /// Where the user drag gesture started in terms of which floor
        /// </summary>
        public int ToolDragFloor => toolDragFloor;

        //Protected
        protected bool DeleteMode { get; set; }
        protected static GameObject ToolCursorObject { get; set; }
        protected static Transform ToolCursorObjectTransform => ToolCursorObject.transform;
        protected static GameObject SelectionStartToolCursor { get; set; }
        /// <summary>
        /// See: <see cref="MultiLevelBehavior"/>
        /// </summary>
        protected virtual MultilevelBehavior MultiLevelBehavior { get; set; } = MultilevelBehavior.Deny;

        protected BuildModeServer BuildModeServer { get; }

        protected AbstractRegionSelectBuildTool(BuildModeServer Server) : base()
        {
            BuildModeServer = Server;
            Init();
        }

        protected virtual void Init()
        {
            //get the default Wand
            if (ToolCursorObject == null)
                ToolCursorObject = GameObject.Find("Wand");
            if (SelectionStartToolCursor == null)
            {
                SelectionStartToolCursor = GameObject.Find("debugWandCompanion");
                SelectionStartToolCursor.SetActive(false);
            }
        }

        protected override void OnActiveChanged(bool NewValue)
        {
            ToolCursorObject.SetActive(NewValue);
            if (!NewValue) SelectionStartToolCursor.SetActive(false);
        }

        public override void OnToolStart(BuildToolContext Context)
        {
            if (IsHolding) return; // huh? weird edge case here
            IsHolding = true;
            toolDragStart = toolDragEnd = toolLastActionDragEnd = Context.GridPosition;
            toolDragFloor = Context.CursorFloor;

            //set the selection origin cursor
            toolDragStartWorldPos = Context.WandPosition;
            SelectionStartToolCursor.SetActive(true);
            SelectionStartToolCursor.transform.position = Context.WandPosition;
            return;
        }

        public override void OnToolUpdate(BuildToolContext Context)
        {
            base.OnToolUpdate(Context);

            var dirtyToolDragEnd = toolDragEnd;

            if (!IsActive) return;
            ToolCursorObjectTransform.position = Context.WandPosition;
            if (!IsHolding) return;
            toolDragEnd = Context.GridPosition;

            if (MultiLevelBehavior == MultilevelBehavior.CancelAction && toolDragFloor != Context.CursorFloor)
            {// drag between floors!
                OnToolCancel("Start and end positions are not at the same level.");
                return;
            }

            //check if player change wall creation type
            CheckDeleteMode();

            if(dirtyToolDragEnd != toolDragEnd)
                //clear any created wall facades
                DoHoverAction(true);
            toolLastActionDragEnd = toolDragEnd;

            //User let go of the Wand, signal finalize event
            if (!Input.GetMouseButton(0))
            { // finalize
                OnToolFinalize(Context);
                return;
            }

            if (dirtyToolDragEnd != toolDragEnd)
                //Update facade
                DoHoverAction();
        }

        public override void OnToolFinalize(BuildToolContext Context)
        {
            IsHolding = false; // finish drag gesture
            if (toolDragStart == toolDragEnd) return; // misclick
            if ((MultiLevelBehavior == MultilevelBehavior.CancelAction || MultiLevelBehavior == MultilevelBehavior.Deny)
                && toolDragFloor != Context.CursorFloor)
            {// drag between floors!
                OnToolCancel("Start and end positions are not at the same level.");
                return;
            }            

            //check if player change creation type
            CheckDeleteMode();
            //Create / Delete the area
            DoAction();

            SelectionStartToolCursor.SetActive(false); // hide cursor
            IsHolding = false; // drop tool       

            base.OnToolFinalize(Context);
        }

        public override void OnToolCancel(string Reason)
        {
            DoHoverAction(true);

            SelectionStartToolCursor.SetActive(false); // hide cursor
            IsHolding = false; // drop tool

            Debug.Log($"{ToolName} cancelled. Reason: {Reason ?? "none given"}");
        }

        protected virtual void CheckDeleteMode()
        {
            //check if deleting objects
            DeleteMode = false;
            if (Input.GetKey(KeyCode.LeftControl)) DeleteMode = true;
        }
        /// <summary>
        /// Performs the action that this tool advertises.
        /// <para/>Note: This is not the same thing as <see cref="OnToolFinalize(BuildToolContext)"/> as that is distinctly
        /// when the user finishes the action. <see cref="DoAction(bool)"/> can be called many times depending on the implementation.
        /// </summary>
        /// <param name="Undo"></param>
        protected abstract void DoAction(bool Undo = false);
        /// <summary>
        /// Same as <see cref="DoAction(bool)"/> except with the intention of previewing the action before committing it
        /// </summary>
        /// <param name="Undo"></param>
        protected virtual void DoHoverAction(bool Undo = false) => DoAction(Undo);
    }
}

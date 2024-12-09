using OpenTS2.Components;
using OpenTS2.Scenes.Lot;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Engine.Modes.Build.Tools
{
    //*** NEEDS A REWORK ONCE SCENEGRAPH RENDERING IS REWORKED BECAUSE THIS IS NOT A GOOD SOLUTION
    internal class HandTool : AbstractBuildTool
    {
        private LotLoad loadedLot;
        private LotArchitecture architecture;

        private Transform holdingObjectRoot;
        private GameObject holdingObjectReference;
        private Vector3 originalPosition;
        bool IsHoldingObject => holdingObjectReference != null;

        public override string ToolName => "Hand Tool";
        public override BuildTools ToolType => BuildTools.Hand;

        /// <summary>
        /// Creates a new <see cref="HandTool"/> on the specified lot
        /// </summary>
        /// <param name="loadedLot"></param>
        /// <param name="architecture"></param>
        public HandTool(LotLoad loadedLot, LotArchitecture architecture)
        {
            this.loadedLot = loadedLot;
            this.architecture = architecture;

            Init();
        }

        private void Init()
        {
            
        }

        public override void OnToolCancel(string Reason)
        {
            if (!IsHoldingObject) return;

            holdingObjectRoot.position = originalPosition;

            holdingObjectReference = null;
            holdingObjectRoot = null;

            Debug.Log($"{ToolName} cancelled. Reason: {Reason ?? "null"}");
        }

        public override void OnToolUpdate(BuildToolContext Context)
        {
            base.OnToolUpdate(Context);

            if (!IsHoldingObject) return;
            holdingObjectRoot.position = Context.WandPosition;

            if (Input.GetMouseButtonDown(0))
                OnToolFinalize(Context);
        }

        public override void OnToolFinalize(BuildToolContext Context)
        {
            if (!IsHoldingObject) return;

            holdingObjectRoot.position = Context.WandPosition;

            holdingObjectReference = null;
            holdingObjectRoot = null;

            base.OnToolFinalize(Context);
        }

        public override void OnToolStart(BuildToolContext Context)
        {
            // PERFORM OBJECT HITTEST (The colliders are added in ScenegraphComponent)
            Ray camRay = TestLotViewCamera.GetMouseCursor3DRay(Camera.main);
            var hits = Physics.RaycastAll(camRay);

            if (hits.Any())
            { // find any hits
                foreach (RaycastHit hit in hits.OrderBy(x => x.distance))
                {
                    var parent = hit.collider.gameObject.transform.parent.gameObject;
                    if (parent.GetComponent<ScenegraphComponent>() != null)
                    {
                        holdingObjectReference = parent;
                        holdingObjectRoot = holdingObjectReference.transform;
                        originalPosition = holdingObjectRoot.position;
                        break;
                    }
                }
            }
        }

        protected override void OnActiveChanged(bool NewValue)
        {
            
        }
    }
}

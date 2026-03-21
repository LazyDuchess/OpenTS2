using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts.Lot
{
    public class LotViewHUDController : MonoBehaviour
    {
        //UI Parts
        public LotViewBuildModeHUD BuildModePanel { get; private set; }
        public LotViewHUD Puck { get; private set; }

        void Start()
        {
            BuildModePanel = new LotViewBuildModeHUD();
            Puck = new LotViewHUD();
            Puck.OnModeChanged += GizmoModeChanged;
        }

        private void GizmoModeChanged(object sender, LotViewHUD.HUD_UILotModes e)
        {
            //TODO: Make UI react to mode changes ... for now not really necessary
        }

        void Update()
        {
            
        }
    }
}

using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts
{
    public class NeighborhoodHUD : UILayoutInstance
    {
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);

        public NeighborhoodHUD() : this(MainCanvas)
        {

        }

        public NeighborhoodHUD(Transform canvas) : base(canvas)
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Stretch);
            var puck = root.GetChildByID(0x4BE6ED7D);
            puck.SetAnchor(UIComponent.AnchorType.BottomLeft);
        }
    }
}

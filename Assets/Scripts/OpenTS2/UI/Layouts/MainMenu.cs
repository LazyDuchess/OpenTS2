using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts
{
    public class MainMenu : UILayoutInstance
    {
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);
        
        public MainMenu() : this(MainCanvas)
        {
            
        }

        public MainMenu(Transform canvas) : base(canvas)
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Center);
            root.transform.SetAsFirstSibling();

            var background = root.GetChildByID(0x0DA36C7D);
            background.gameObject.SetActive(true);
            background.SetAnchor(UIComponent.AnchorType.Center);
            background.transform.SetAsFirstSibling();

            var upperLeftSim = root.GetChildByID(0xE1) as UIBMPComponent;
            var lowerRightSim = root.GetChildByID(0xE3) as UIBMPComponent;

            // IDs for the textures for the Sims are stored in a constants table UI element.
            var constantsTable = root.GetChildByID(0x4DC1DCE2);
            var constantComponents = constantsTable.Children;

            var upperLeftKeys = new List<ResourceKey>();
            var lowerRightKeys = new List<ResourceKey>();

            // Read the constants inside the constants table.
            foreach(var uiComponent in constantComponents)
            {
                var constant = UIUtils.GetConstant(uiComponent.Element.Caption);
                var isUpperLeft = false;
                var valid = false;
                switch(constant.Key)
                {
                    case "kUpperLeft":
                        isUpperLeft = true;
                        valid = true;
                        break;
                    case "kLowerRight":
                        isUpperLeft = false;
                        valid = true;
                        break;
                }
                if (!string.IsNullOrWhiteSpace(constant.Value) && valid)
                {
                    var stringlist = UIUtils.GetCharSeparatedList(constant.Value, ';');
                    foreach(var str in stringlist)
                    {
                        var instanceID = Convert.ToUInt32(str, 16);
                        var key = new ResourceKey(instanceID, 0x499DB772, TypeIDs.IMG);
                        if (isUpperLeft)
                            upperLeftKeys.Add(key);
                        else
                            lowerRightKeys.Add(key);
                    }
                }
            }

            var contentProvider = ContentProvider.Get();

            // Assign random images to the Sims.
            var upperLeftKey = RandomUtils.RandomFromList(upperLeftKeys);
            var lowerRightKey = RandomUtils.RandomFromList(lowerRightKeys);

            upperLeftSim.SetTexture(contentProvider.GetAsset<TextureAsset>(upperLeftKey));
            lowerRightSim.SetTexture(contentProvider.GetAsset<TextureAsset>(lowerRightKey));
            upperLeftSim.Color = Color.white;
            lowerRightSim.Color = Color.white;
        }
    }
}

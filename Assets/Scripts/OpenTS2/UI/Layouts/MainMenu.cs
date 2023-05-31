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
    public class MainMenu
    {
        public UIComponent Root;
        private static ResourceKey s_mainMenuKey = new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);

        public MainMenu(Transform canvas)
        {
            var contentProvider = ContentProvider.Get();
            var mainMenuLayout = contentProvider.GetAsset<UILayout>(s_mainMenuKey);
            var components = mainMenuLayout.Instantiate(canvas);

            Root = components[0];
            Root.SetAnchor(UIComponent.AnchorType.Center);
            Root.transform.SetAsFirstSibling();

            var background = Root.GetChildByID(0x0DA36C7D);
            background.gameObject.SetActive(true);
            background.transform.SetParent(Root.transform.parent);
            background.SetAnchor(UIComponent.AnchorType.Center);
            background.transform.SetAsFirstSibling();
        }
    }
}

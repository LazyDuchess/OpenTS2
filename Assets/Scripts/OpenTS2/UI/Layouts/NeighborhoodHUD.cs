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
    /// <summary>
    /// Neighborhood view HUD.
    /// </summary>
    public class NeighborhoodHUD : UILayoutInstance
    {
        private const uint LargeCityNameTextID = 0x3;
        private const uint MediumCityNameTextID = 0xB3;
        private const uint SmallCityNameTextID = 0xA3;
        private const uint PrimaryOptionsID = 0x4BE6ED7E;
        private const uint MainMenuID = 0x2;
        private const uint PuckID = 0x4BE6ED7D;
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);
        private MainMenu _mainMenu = null;

        public NeighborhoodHUD() : this(MainCanvas)
        {

        }

        public NeighborhoodHUD(Transform canvas) : base(canvas)
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Stretch);
            var puck = root.GetChildByID(PuckID);
            puck.SetAnchor(UIComponent.AnchorType.BottomLeft);

            var primaryOptions = root.GetChildByID(PrimaryOptionsID, false);

            var mainMenu = root.GetChildByID<UIButtonComponent>(MainMenuID, false);
            mainMenu.OnClick += OnMainMenu;

            var largeCityName = primaryOptions.GetChildByID<UITextComponent>(LargeCityNameTextID);
            largeCityName.Text = NeighborhoodManager.CurrentNeighborhood.GetLocalizedName();
        }

        void OnMainMenu()
        {
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            if (_mainMenu != null)
                return;
            Components[0].gameObject.SetActive(false);
            _mainMenu = new MainMenu(true);
            _mainMenu.OnClose += OnMainMenuClose;
            CursorController.Cursor = CursorController.CursorType.Default;
        }

        void OnMainMenuClose()
        {
            Components[0].gameObject.SetActive(true);
            _mainMenu = null;
        }
    }
}

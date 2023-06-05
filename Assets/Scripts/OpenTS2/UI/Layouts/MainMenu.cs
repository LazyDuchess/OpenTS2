using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI.Layouts
{
    /// <summary>
    /// Main Menu UI, with neighborhood chooser.
    /// </summary>
    public class MainMenu : UILayoutInstance
    {
        private UIComponent _shade;
        private NeighborhoodPreview _neighborhoodPreview;
        private List<Neighborhood> _neighborHoods;
        private NeighborhoodIcon[] _neighborhoodIcons;
        private int _currentPage = 0;
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

            var upperLeftSim = root.GetChildByID<UIBMPComponent>(0xE1);
            var lowerRightSim = root.GetChildByID<UIBMPComponent>(0xE3);

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
            var keyAmount = Mathf.Min(upperLeftKeys.Count, lowerRightKeys.Count);
            var simTextureIndex = UnityEngine.Random.Range(0, keyAmount);

            var upperLeftKey = upperLeftKeys[simTextureIndex];
            var lowerRightKey = lowerRightKeys[simTextureIndex];

            upperLeftSim.SetTexture(contentProvider.GetAsset<TextureAsset>(upperLeftKey));
            lowerRightSim.SetTexture(contentProvider.GetAsset<TextureAsset>(lowerRightKey));
            upperLeftSim.Color = Color.white;
            lowerRightSim.Color = Color.white;

            CreateNeighborhoodIcons();
            UpdateNeighborhoods();

            // Unparent shade and move it to the top, so that it covers the main menu fully.

            _shade = Components[0].GetChildByID(0x100A);
            _shade.transform.parent = MainCanvas;
            _shade.transform.SetAsLastSibling();
            _shade.SetAnchor(UIComponent.AnchorType.Center);

            // Set up the neighborhood previews.

            _neighborhoodPreview = new NeighborhoodPreview();
            _neighborhoodPreview.Hide();
            _neighborhoodPreview.OnShow += Shade;
            _neighborhoodPreview.OnHide += Unshade;
        }

        void Shade()
        {
            _shade.gameObject.SetActive(true);
        }

        void Unshade()
        {
            _shade.gameObject.SetActive(false);
        }

        // Set up the neighborhood icon grid and controls.
        void CreateNeighborhoodIcons()
        {
            _neighborHoods = NeighborhoodManager.Neighborhoods.Where((neighborhood) => neighborhood.Prefix != "Tutorial" && neighborhood.NeighborhoodType == Neighborhood.Type.Main).ToList();

            var thumbsRectangle = Components[0].GetChildByID(0x00001006);
            var thumbsCenter = thumbsRectangle.GetCenter();

            _neighborhoodIcons = new NeighborhoodIcon[3];

            _neighborhoodIcons[1] = CreateNeighborhoodIcon(thumbsCenter);
            var offset = _neighborhoodIcons[1].Components[0].RectTransformComponent.sizeDelta.x;
            _neighborhoodIcons[0] = CreateNeighborhoodIcon(thumbsCenter - new Vector2(offset, 0f));
            _neighborhoodIcons[2] = CreateNeighborhoodIcon(thumbsCenter + new Vector2(offset, 0f));

            var previousButton = Components[0].GetChildByID<UIButtonComponent>(0x1004);
            var nextButton = Components[0].GetChildByID<UIButtonComponent>(0x1005);
            var quitButton = Components[0].GetChildByID<UIButtonComponent>(0xA5);
            previousButton.OnClick += OnPrevPage;
            nextButton.OnClick += OnNextPage;
            quitButton.OnClick += OnQuit;
        }

        // Creates a clickable neighborhood icon, displays preview on click.
        NeighborhoodIcon CreateNeighborhoodIcon(Vector2 position)
        {
            var icon = new NeighborhoodIcon(MainCanvas);
            var root = icon.Components[0];
            root.SetAnchor(UIComponent.AnchorType.Center);
            root.SetPositionCentered(position);
            root.transform.SetSiblingIndex(Components[0].transform.GetSiblingIndex() + 1);
            icon.OnClick += OnNeighborhoodIconClick;
            return icon;
        }

        // Display a neighborhood preview when an icon is clicked.
        void OnNeighborhoodIconClick(Neighborhood neighborhood)
        {
            if (neighborhood == null)
                return;
            _neighborhoodPreview.SetNeighborhood(neighborhood);
            _neighborhoodPreview.Show();
        }

        void OnQuit()
        {
            ContentProvider.Get().Changes.SaveChanges();
            Application.Quit();
        }

        void OnPrevPage()
        {
            _currentPage--;
            UpdateNeighborhoods();
        }

        void OnNextPage()
        {
            _currentPage++;
            UpdateNeighborhoods();
        }

        // Update neighborhood grid with current page, and disable/enable previous/next buttons.
        void UpdateNeighborhoods()
        {
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            var iconAmount = _neighborhoodIcons.Length;
            var iterationStartsFrom = _currentPage * iconAmount;
            for(var i=0;i<iconAmount;i++)
            {
                var currentI = iterationStartsFrom + i;
                var component = _neighborhoodIcons[i].Components[0];
                if (currentI >= _neighborHoods.Count)
                {
                    component.gameObject.SetActive(false);
                    continue;
                }
                var neighborhood = _neighborHoods[currentI];
                component.gameObject.SetActive(true);
                _neighborhoodIcons[i].SetNeighborhood(neighborhood);
            }
            var previousButton = Components[0].GetChildByID(0x1004);
            var nextButton = Components[0].GetChildByID(0x1005);
            previousButton.gameObject.SetActive(CanGoPreviousPage());
            nextButton.gameObject.SetActive(CanGoNextPage());
            CursorController.Cursor = CursorController.CursorType.Default;
        }

        bool CanGoPreviousPage()
        {
            if (_currentPage > 0)
                return true;
            return false;
        }

        bool CanGoNextPage()
        {
            var iconAmount = _neighborhoodIcons.Length;
            var pageAmount = Mathf.CeilToInt((float)_neighborHoods.Count / iconAmount);
            if (_currentPage >= pageAmount - 1)
                return false;
            return true;
        }
    }
}

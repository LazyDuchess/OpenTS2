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
    public class MainMenu : UILayoutInstance
    {
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
        }

        void CreateNeighborhoodIcons()
        {
            _neighborHoods = NeighborhoodManager.Neighborhoods.Where((neighborhood) => neighborhood.Folder != "Tutorial").ToList();

            var thumbsRectangle = Components[0].GetChildByID(0x00001006);
            var thumbsCenter = thumbsRectangle.GetCenter();

            _neighborhoodIcons = new NeighborhoodIcon[3];

            _neighborhoodIcons[0] = new NeighborhoodIcon(MainCanvas);
            var offset = _neighborhoodIcons[0].Components[0].RectTransformComponent.sizeDelta.x;
            _neighborhoodIcons[0].Components[0].SetAnchor(UIComponent.AnchorType.Center);
            _neighborhoodIcons[0].Components[0].SetPositionCentered(thumbsCenter - new Vector2(offset, 0f));
            _neighborhoodIcons[0].Components[0].transform.SetSiblingIndex(Components[0].transform.GetSiblingIndex() + 1);

            _neighborhoodIcons[1] = new NeighborhoodIcon(MainCanvas);
            _neighborhoodIcons[1].Components[0].SetAnchor(UIComponent.AnchorType.Center);
            _neighborhoodIcons[1].Components[0].SetPositionCentered(thumbsCenter);
            _neighborhoodIcons[1].Components[0].transform.SetSiblingIndex(Components[0].transform.GetSiblingIndex() + 1);

            _neighborhoodIcons[2] = new NeighborhoodIcon(MainCanvas);
            _neighborhoodIcons[2].Components[0].SetAnchor(UIComponent.AnchorType.Center);
            _neighborhoodIcons[2].Components[0].SetPositionCentered(thumbsCenter + new Vector2(offset, 0f));
            _neighborhoodIcons[2].Components[0].transform.SetSiblingIndex(Components[0].transform.GetSiblingIndex() + 1);

            var previousButton = Components[0].GetChildByID(0x1004) as UIButtonComponent;
            var nextButton = Components[0].GetChildByID(0x1005) as UIButtonComponent;
            var quitButton = Components[0].GetChildByID(0x000000A5) as UIButtonComponent;
            previousButton.OnClick += OnPrevPage;
            nextButton.OnClick += OnNextPage;
            quitButton.OnClick += OnQuit;
        }

        void OnQuit()
        {
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
                var thumbnailBMP = component.GetChildByID(0xA1) as UIBMPComponent;
                var nameText = component.GetChildByID(0xA5);
                nameText.GetComponent<Text>().text = neighborhood.GetLocalizedName();
                thumbnailBMP.Color = Color.white;
                thumbnailBMP.RawImageComponent.texture = neighborhood.Thumbnail;
                var reiaPlayer = thumbnailBMP.gameObject.GetComponent<ReiaPlayer>();
                if (reiaPlayer == null)
                    reiaPlayer = thumbnailBMP.gameObject.AddComponent<ReiaPlayer>();
                var reiaPath = neighborhood.ReiaPath;
                reiaPlayer.Stop();
                FlipTransformForBMP(thumbnailBMP.RectTransformComponent);
                if (File.Exists(reiaPath))
                {
                    FlipTransformForReia(thumbnailBMP.RectTransformComponent);
                    var reiaStream = File.OpenRead(reiaPath);
                    reiaPlayer.SetReia(reiaStream, true);
                    reiaPlayer.Speed = 1f;
                }
            }
            var previousButton = Components[0].GetChildByID(0x1004);
            var nextButton = Components[0].GetChildByID(0x1005);
            previousButton.gameObject.SetActive(CanGoPreviousPage());
            nextButton.gameObject.SetActive(CanGoNextPage());
            CursorController.Cursor = CursorController.CursorType.Default;
        }

        void FlipTransformForReia(RectTransform transform)
        {
            if (transform.localScale.y == -1f)
                return;
            transform.localScale = new Vector3(1f, -1f, 1f);
            transform.position -= new Vector3(0f, transform.sizeDelta.y, 0f);
        }

        void FlipTransformForBMP(RectTransform transform)
        {
            if (transform.localScale.y == 1f)
                return;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.position += new Vector3(0f, transform.sizeDelta.y, 0f);
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
            var pageAmount = Mathf.CeilToInt(_neighborHoods.Count / iconAmount);
            if (_currentPage >= pageAmount)
                return false;
            return true;
        }
    }
}

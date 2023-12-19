using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine.Modes.Build;
using OpenTS2.Engine.Modes.Build.Tools;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI.Layouts.Lot
{
    /// <summary>
    /// This is a sub-control of the <see cref="LotViewHUD"/> with functionality serving the Build Mode menus in the Lot View UI puck
    /// </summary>
    public class LotViewBuildModeHUD : UILayoutInstance
    {
        class BuildHUD_CatalogItemComponent : UILayoutInstance
        {
            //PRIVATE
            const uint BaseID = 0xDAFE6503;
            const uint PreviewID = 0x00000001;
            private CatalogObjectAsset item;
            public CatalogObjectAsset SelectedItem => item;

            Material displayMaterial;

            //PROTECTED
            protected override ResourceKey UILayoutResourceKey => new ResourceKey(BaseID, 0xA99D8A11, TypeIDs.UI);

            //PUBLIC
            public event EventHandler<CatalogObjectAsset> Clicked;

            public BuildHUD_CatalogItemComponent(Transform parent) : base(parent)
            {
                Init();
            }

            void Init()
            {
                Components[0].GetChildByID<UIButtonComponent>(0x03).OnClick += delegate
                {
                    Clicked?.Invoke(this, SelectedItem);
                };
            }

            /// <summary>
            /// Changes this component's displayed texture and attached catalog asset
            /// </summary>
            /// <param name="Asset"></param>
            public void Display(CatalogObjectAsset Asset)
            {
                if(displayMaterial != null)
                {
                    UnityEngine.Object.Destroy(displayMaterial);
                    displayMaterial = null;
                }

                item = Asset;

                var content = ContentProvider.Get();
                var material = content.GetAsset<ScenegraphMaterialDefinitionAsset>(
                    new ResourceKey($"{Asset.Material}_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXMT));

                var preview = Components[0].GetChildByID(PreviewID);
                preview.RectTransformComponent.localPosition = new Vector3(8, -8);
                preview.RectTransformComponent.sizeDelta = new Vector2(34,34);
                preview.gameObject.SetActive(true);
                
                //Set preview image
                var bmp = preview.GetComponent<RawImage>();
                displayMaterial = material.GetAsUnityMaterial();
                bmp.texture = displayMaterial.mainTexture;                
            }
        }

        /// <summary>
        /// Contains the state of the catelog UI flyout
        /// </summary>
        class BuildHUD_CatalogComponent
        {
            const int XMARGIN = 10, YMARGIN = 5; // Margins between catalog items in the menu
            internal const uint CatalogItemsExtender = 0xAD502BBF;
            const uint CatalogItemsComponent = 0x00001006;
            const uint PreviousButton = 0x00001004;
            const uint NextButton = 0x00001005;

            UIComponent rootElement;
            internal bool opened { get; private set; }

            private string subsort = "all";
            int itemsPerPage = 6, currentPage = 1;

            List<CatalogObjectAsset> items = new List<CatalogObjectAsset>();
            /// <summary>
            /// The list of reusable UIComponent instances that will dynamically update their previews depending on which page we're on
            /// </summary>
            List<BuildHUD_CatalogItemComponent> controls = new List<BuildHUD_CatalogItemComponent>();

            public BuildHUD_CatalogComponent(UIComponent CatalogComponent)
            {
                rootElement = CatalogComponent;

                Init();
                Close();
            }

            void Init()
            {
                var catalogItemsComponent = rootElement.GetChildByID(CatalogItemsComponent);
                catalogItemsComponent.gameObject.SetActive(true);
                catalogItemsComponent.GetComponent<RawImage>().enabled = false; // background is buggy turn it off

                for(int item = 0; item < itemsPerPage; item++) // create dynamic items
                {
                    int x = item / 2;
                    int y = item - (x * 2);

                    var control = new BuildHUD_CatalogItemComponent(catalogItemsComponent.transform);
                    var ctrlTransform = control.Components[0].RectTransformComponent;
                    ctrlTransform.localPosition = new Vector3( // handle padding here
                        (ctrlTransform.sizeDelta.x + XMARGIN) * x, -(ctrlTransform.sizeDelta.y + YMARGIN) * y);
                    controls.Add(control);

                    //CLICK EVENT
                    control.Clicked += CatalogItemSelected;
                }

                //Rig up next and prev buttons
                var prevButton = rootElement.GetChildByID<UIButtonComponent>(PreviousButton);
                prevButton.OnClick += Previous;
                var nextButton = rootElement.GetChildByID<UIButtonComponent>(NextButton);
                nextButton.OnClick += Advance;
            }

            private void CatalogItemSelected(object sender, CatalogObjectAsset e)
            {
                //If the current build tool is compatible with patterns -- invoke it
                (BuildModeServer.Get().CurrentTool as IPatternSelectableBuildTool)?.SetPaintPattern(e.Material);
            }

            void Advance() => SwitchPage(currentPage + 1);
            void Previous() => SwitchPage(currentPage - 1);
            /// <summary>
            /// Switches to the given page in the catalog by updating the dynamic controls to the new page's contents
            /// </summary>
            /// <param name="PageNumber"></param>
            void SwitchPage(int PageNumber)
            {
                currentPage = PageNumber;
                if (currentPage < 0) currentPage = 0;

                int startIndex = currentPage * itemsPerPage;

                var itemsFilter = (IEnumerable<CatalogObjectAsset>)items;
                if (subsort == "ots2_hiddentiles")
                {
                    itemsFilter = items.Where(x => ((Uint32Prop)x.Properties.Properties["showincatalog"]).Value == 0);
                }
                else if (subsort != "all") // filter the itemssource if applicable (ignore this filter if "all" is selected)
                    itemsFilter = items.Where(x => ((StringProp)x.Properties.Properties["subsort"]).Value == subsort);

                for (int item = 0; item < itemsPerPage; item++)
                {
                    //Set buttons hidden at first until some content to show has been found
                    var control = controls[item];
                    control.Components[0].gameObject.SetActive(false);

                    //attempt to load the content item
                    var assetRef = itemsFilter.ElementAtOrDefault(startIndex + item);
                    if (assetRef == null) continue; // no content here
                    //found some content, make the button visible and display a preview of the catalog item
                    control.Components[0].gameObject.SetActive(true);
                    control.Display(assetRef);
                }
            }

            public void Close()
            {
                rootElement.gameObject.SetActive(false);
                opened = false;                
            }
            public void Open(string CatelogItemType, string Subsort = "all")
            {
                if (string.IsNullOrWhiteSpace(Subsort))
                    Subsort = "all";

                rootElement.gameObject.SetActive(true);
                opened = true;

                //Update the selection of items to match the definition
                subsort = Subsort;
                UpdateItemsSource(CatalogManager.Get().Objects.Where(x => x.Type == CatelogItemType));
            }
            public void SetLocalPosition(Vector3 Position) => 
                rootElement.RectTransformComponent.localPosition = Position;

            /// <summary>
            /// Updates the catalog control's items source property.
            /// <para/>Use this when changing the selection of items being shown in the catelog.
            /// <para/>You do not need to account for multiple pages of items, that will be handled automatically by the control.
            /// </summary>
            void UpdateItemsSource(IEnumerable<CatalogObjectAsset> CatalogItems)
            {
                items.Clear();
                items.AddRange(CatalogItems);

                SwitchPage(0); // switch to page one of the new data
            }

            /// <summary>
            /// Sets a filter on the ItemsSource.
            /// <para>See: <see cref="UpdateItemsSource(IEnumerable{CatalogObjectAsset})"/> and <see cref="Open(string)"/></para>
            /// </summary>
            /// <param name="Subsort"></param>
            /// <exception cref="NotImplementedException"></exception>
            internal void SetSubsort(string Subsort)
            {
                subsort = Subsort;
                SwitchPage(0);
            }
        }

        #region TopLevelHUDElements

        /// <summary>
        /// Sorts (pages) for different tools in the build mode hud
        /// </summary>
        enum BuildHUD_Sorts : uint
        {
            TopLevel = 0x0000A000,
            Walls = 0x0000B100,
            Foundations = 0x0000B200,
            DoorsWindowsArches = 0x0000B300,
            Floors = 0x0000B400,
            WallPaints = 0x0000B500,
            Stairs = 0x0000B600,
            Terrain = 0x0000B700,
            GardenCenter = 0x0000B800,
            Roofs = 0x0000B900,
            Garages = 0x0000BB00,
            Architecture = 0x0000BC00,
            Misc = 0x0000BA00
        }
        enum BuildHUD_CollectionPages : uint
        {
            Normal = 0x000000A8
        }
        enum BuildHUD_BasicToolButtons : uint
        {
            Hand = 0x000000A5,
            EyedropperTool = 0x000000A4
        }

        #endregion

        #region Subsorts

        enum BuildHUD_TopLevelButtons : uint
        {
            TopLevelSortButton = 0x000000A6,
            WallsSortButton = 0x0000A100,
            FoundationsSortButton = 0x0000A200,
            DoorsAndWindowsSortButton = 0x0000A300,
            FloorsSortButton = 0x0000A400,
            WallPaintsSortButton = 0x0000A500,
            StairsSortButton = 0x0000A600,
            TerrainCenterButton = 0x0000A700,
            GardenCenterButton = 0x0000A800,
            RoofsSortButton = 0x0000A900,
            MiscButton = 0x0000AA00,
            GarageButton = 0x0000AB00,
            ArchitectureSortButton = 0x0000AC00,
        }
        enum BuildHUD_WallsSortButtons : uint
        {
            WallsNRooms = 0x0000B120,
            HalfWalls = 0x0000B130
        }
        enum BuildHUD_TerrainSortButtons : uint
        {
            ElevationTools = 0x0000B720,
            TerrainPaints = 0x0000B730, // called 'grass' in UI, pretty cool :0
            Water = 0x0000B740,
            FlattenLot = 0x000000AD,              
        }
        enum BuildHUD_TerrainBrushSizeButtons : uint
        {
            SmallSizeButton = 0x0000B751,
            MedSizeButton = 0x0000B752,
            LgSizeButton = 0x0000B753,
        }
        enum BuildHUD_FloorsSubsortButtons : uint
        {
            StoneButton = 0x0000B420,
            BrickButton = 0x0000B430,
            WoodButton = 0x0000B440,
            CarpetButton = 0x0000B450,
            TileButton = 0x0000B460,
            LinoleumButton = 0x0000B470,
            PouredButton = 0x0000B480,
            OtherButton = 0x0000B490,
            AllButton = 0x0000B4A0,
        }

        #endregion

        const uint BaseID = 0x49060003;
        const uint TileBG = 0xB4; // UI Background Panel
        const uint EndCap = 0x000000B5; // UI Curvy End Cap
        const uint ProdCatelogFlyout = 0x49063004;
        const uint AutoRoofOptionsExtender = 0x000000AC;        
        const uint TerrainPageSizesGroup = 0x0000B750;

        protected override ResourceKey UILayoutResourceKey => new ResourceKey(BaseID, 0xA99D8A11, TypeIDs.UI);
        UIComponent rootElement => Components[0];

        //Pages (Sorts)

        /// <summary>
        /// Contains information for each sort page in build mode to display it correctly
        /// </summary>
        class BuildHUDSort
        {
            /// <summary>
            /// The enum type that has the IDs for the buttons that are inside of this sort that should be Toggled/Untoggled
            /// after the sort is opened and dismissed
            /// </summary>
            public Type ButtonIDsEnum { get; set; } = null;
            /// <summary>
            /// The UI ID for this sort
            /// </summary>
            public uint SortGroupID { get; set; }

            //CATALOG PROVIDER 
            public string CatalogItemType { get; set; }
            public bool HasCatalogItems => CatalogItemType != null;
            public string CatalogDefaultSubsort { get; set; } = "all";

            public BuildHUDSort(Type buttonIDsEnum, uint sortGroupID)
            {
                ButtonIDsEnum = buttonIDsEnum;
                SortGroupID = sortGroupID;
            }
        }

        BuildHUDSort CurrentSort;
        /// <summary>
        /// Links Sort buttons to their corresponding pages in the interface
        /// <para>Wall Button links to the Wall Subpage with Half Walls, Regular Walls, Diagonal Room, etc.</para>
        /// </summary>
        Dictionary<BuildHUD_Sorts, BuildHUDSort> sortPageMap = new Dictionary<BuildHUD_Sorts, BuildHUDSort>()
        {
            { BuildHUD_Sorts.TopLevel, new BuildHUDSort(typeof(BuildHUD_TopLevelButtons), (uint)BuildHUD_Sorts.TopLevel) },
            { BuildHUD_Sorts.Walls, new BuildHUDSort(typeof(BuildHUD_WallsSortButtons), (uint)BuildHUD_Sorts.Walls) },
            { BuildHUD_Sorts.Terrain, new BuildHUDSort(typeof(BuildHUD_TerrainSortButtons), (uint)BuildHUD_Sorts.Terrain) },
            { BuildHUD_Sorts.Floors, new BuildHUDSort(typeof(BuildHUD_FloorsSubsortButtons), (uint)BuildHUD_Sorts.Floors)
                { // activate the catelog page for this sort
                    CatalogItemType = CatalogObjectType.Floor,                    
                }
            },
        };

        // UI buttons and event system
        Dictionary<uint, Action> buildHUDButtonCallbacks; // built at runtime
        /// <summary>
        /// All UI buttons added to the event system (See: <see cref="BuildUIEventsMap"/>) will set this value to 
        /// themselves prior to the event being called (May change later)
        /// </summary>
        UIButtonComponent CurrentButtonEventContext;
        BuildHUD_CatalogComponent Catalog;

        public bool CatalogOpen => Catalog.opened;

        public LotViewBuildModeHUD() : this(MainCanvas)
        {

        }
        public LotViewBuildModeHUD(Transform Canvas) : base(Canvas)
        {
            Init();
            BuildUIEventsMap();
            WireUpUIEvents();
        }

        void Init()
        {
            //Position in bot left
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.BottomLeft);
            root.transform.position = new Vector3(105, root.transform.position.y, 0);

            //fix sizing
            var iconButton = root.GetChildByID((uint)BuildHUD_BasicToolButtons.Hand);
            iconButton.RectTransformComponent.sizeDelta = new Vector2(30, 30);
            iconButton = root.GetChildByID((uint)BuildHUD_BasicToolButtons.EyedropperTool);
            iconButton.RectTransformComponent.sizeDelta = new Vector2(30, 30);

            //turn off all controls except necessary ones
            var autoRoofOptions = root.GetChildByID(AutoRoofOptionsExtender);
            autoRoofOptions.gameObject.SetActive(false);
            Catalog = new BuildHUD_CatalogComponent(root.GetChildByID(BuildHUD_CatalogComponent.CatalogItemsExtender));
        }

        void BuildUIEventsMap()
        {
            buildHUDButtonCallbacks = new Dictionary<uint, Action>()
            {
                // BASIC BUTTONS
                { (uint)BuildHUD_BasicToolButtons.Hand, delegate { ActionChangeTool(BuildTools.Hand); } },

                // TOP LEVEL SORTS BUTTONS
                { (uint)BuildHUD_TopLevelButtons.TopLevelSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.TopLevel); } },
                { (uint)BuildHUD_TopLevelButtons.WallsSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.Walls); } },
                { (uint)BuildHUD_TopLevelButtons.DoorsAndWindowsSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.DoorsWindowsArches); } },
                { (uint)BuildHUD_TopLevelButtons.FloorsSortButton, delegate
                    {
                        ActionChangeSort(BuildHUD_Sorts.Floors);
                        ActionChangeTool(BuildTools.Floor);
                    } 
                },
                { (uint)BuildHUD_TopLevelButtons.WallPaintsSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.WallPaints); } },
                { (uint)BuildHUD_TopLevelButtons.ArchitectureSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.Architecture); } },
                { (uint)BuildHUD_TopLevelButtons.GardenCenterButton, delegate { ActionChangeSort(BuildHUD_Sorts.GardenCenter); } },
                { (uint)BuildHUD_TopLevelButtons.TerrainCenterButton, delegate { ActionChangeSort(BuildHUD_Sorts.Terrain); } },
                { (uint)BuildHUD_TopLevelButtons.GarageButton, delegate { ActionChangeSort(BuildHUD_Sorts.Garages); } },
                { (uint)BuildHUD_TopLevelButtons.MiscButton, delegate { ActionChangeSort(BuildHUD_Sorts.Misc); } },
                { (uint)BuildHUD_TopLevelButtons.RoofsSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.Roofs); } },
                { (uint)BuildHUD_TopLevelButtons.FoundationsSortButton, delegate { 
                    ActionChangeSort(BuildHUD_Sorts.Foundations);
                    ActionChangeTool(BuildTools.Foundation);
                    } 
                },
                { (uint)BuildHUD_TopLevelButtons.StairsSortButton, delegate { ActionChangeSort(BuildHUD_Sorts.Stairs); } },

                // Walls N' Rooms
                { (uint)BuildHUD_WallsSortButtons.WallsNRooms, delegate { ActionChangeTool(BuildTools.Wall); } },

                // Terrain 
                { (uint)BuildHUD_TerrainSortButtons.ElevationTools, delegate { ActionSetTerrainBrushTool(BuildModeServer.TerrainModificationModes.Raise); } },
                { (uint)BuildHUD_TerrainSortButtons.Water, delegate { ActionSetTerrainBrushTool(BuildModeServer.TerrainModificationModes.Water); } },
                { (uint)BuildHUD_TerrainSortButtons.FlattenLot, delegate { BuildModeServer.Get().FlattenLot(); } },
                { (uint)BuildHUD_TerrainBrushSizeButtons.SmallSizeButton, delegate { ActionModifyTerrainBrushSize(0); } },
                { (uint)BuildHUD_TerrainBrushSizeButtons.MedSizeButton, delegate { ActionModifyTerrainBrushSize(TerrainBrushTool.TerrainBrushSizes.Medium); } },
                { (uint)BuildHUD_TerrainBrushSizeButtons.LgSizeButton, delegate { ActionModifyTerrainBrushSize(TerrainBrushTool.TerrainBrushSizes.Large); } },

                //Floors
                { (uint)BuildHUD_FloorsSubsortButtons.StoneButton, delegate { ActionInvokeSubSort("stone"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.BrickButton, delegate { ActionInvokeSubSort("brick"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.WoodButton, delegate { ActionInvokeSubSort("wood"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.PouredButton, delegate { ActionInvokeSubSort("poured"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.CarpetButton, delegate { ActionInvokeSubSort("carpet"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.OtherButton, delegate { ActionInvokeSubSort("other"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.AllButton, delegate { ActionInvokeSubSort("all"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.TileButton, delegate { ActionInvokeSubSort("tile"); } },
                { (uint)BuildHUD_FloorsSubsortButtons.LinoleumButton, delegate { ActionInvokeSubSort("linoleum"); } },
            };
        }

        void WireUpUIEvents()
        {
            foreach(var keyValuePair in buildHUDButtonCallbacks)
            {
                uint compID = keyValuePair.Key;
                var button = rootElement.GetChildByID<UIButtonComponent>(compID);
                button.OnClick += delegate
                {
                    //Set context
                    CurrentButtonEventContext = button;
                    //Invoke event
                    keyValuePair.Value();
                    //Remove context
                    CurrentButtonEventContext = null;
                };
            }
        }
        void EnsureGroupToggled(Type EnumTypeGroup, bool ToggleState = false) => EnsureGroup(EnumTypeGroup, (UIButtonComponent comp) => comp.SetToggled(ToggleState));
        void EnsureGroupHidden(Type EnumUIGroup) => EnsureGroup(EnumUIGroup, (UIComponent comp) => comp.gameObject.SetActive(false));
        void EnsureGroup(Type EnumUIGroup, Action<UIComponent> Modification) => EnsureGroup<UIComponent>(EnumUIGroup, Modification);
        void EnsureGroup<T>(Type EnumUIGroup, Action<T> Modification) where T : UIComponent
        {
            if (EnumUIGroup == null) return;
            foreach (uint groupID in Enum.GetValues(EnumUIGroup))
                Modification(rootElement.GetChildByID<T>(groupID));
        }        
        void CloseCatalog()
        {
            Catalog.Close();
            //Set the endcap visible again
            rootElement.GetChildByID(EndCap).gameObject.SetActive(true);
        }
        void OpenCatalog(string ObjectType, string Subsort = "all")
        {
            Catalog.Open(ObjectType, Subsort);
            //Set the endcap invisible
            rootElement.GetChildByID(EndCap).gameObject.SetActive(false);
        }

        void ActionChangeTool(BuildTools Tool)
        {
            //Untoggle all basic buttons before transitioning            
            EnsureGroupToggled(typeof(BuildHUD_BasicToolButtons));
            if (CurrentSort != default)
                EnsureGroupToggled(CurrentSort.ButtonIDsEnum);
            //set tool button toggled
            if (CurrentButtonEventContext != default)
                CurrentButtonEventContext.SetToggled(true);  
            
            BuildModeServer.Get().ChangeTool(Tool);
        }   

        void ActionChangeSort(BuildHUD_Sorts SortPage)
        {            
            ActionChangeTool(BuildTools.None); // drop current tool

            //Hide all other pages before transitioning            
            EnsureGroupHidden(typeof(BuildHUD_Sorts));
            CloseCatalog();

            var sortRoot = rootElement.GetChildByID((uint)SortPage);
            sortRoot.gameObject.SetActive(true);
            // stretch the ui panel to accomodate for the page we just opened up
            float desiredWidth = sortRoot.RectTransformComponent.sizeDelta.x;
            var TileBGComponent = rootElement.GetChildByID(TileBG).RectTransformComponent; // UI background panel
            TileBGComponent.sizeDelta = new Vector2(desiredWidth, TileBGComponent.sizeDelta.y);
            //move the endcap over so there's no gaps
            var EndCapComp = rootElement.GetChildByID(EndCap).RectTransformComponent; // UI background panel
            EndCapComp.localPosition = TileBGComponent.localPosition + new Vector3(desiredWidth, 0, 0);
            //move the catelog as well -- whether it is active or not
            Catalog.SetLocalPosition(new Vector3(TileBGComponent.localPosition.x + desiredWidth, 0, 0));   
            //catelogComp.sizeDelta = new Vector2(catelogComp.position.x - (MainCanvas.transform as RectTransform).sizeDelta.x - 10, 
            //  catelogComp.sizeDelta.y);

            if (sortPageMap.TryGetValue(SortPage, out var map))
                CurrentSort = map;
            //Untoggle all buttons in the destination sort as well
            EnsureGroupToggled(CurrentSort.ButtonIDsEnum);

            //open the catelog (if applicable)
            if (CurrentSort.HasCatalogItems)
                OpenCatalog(CurrentSort.CatalogItemType, CurrentSort.CatalogDefaultSubsort);
        }

        /// <summary>
        /// Sorts such as Flooring have subsorts (carpet, stone, etc.), this function will update the 
        /// subsort for the <see cref="CurrentSort"/>
        /// </summary>
        /// <param name="Subsort"></param>
        void ActionInvokeSubSort(string Subsort)
        {
            EnsureGroupToggled(CurrentSort.ButtonIDsEnum);
            if (Subsort == "all" && Input.GetKey(KeyCode.LeftShift)) // BuildDEBUG
                Subsort = "ots2_hiddentiles";
            Catalog.SetSubsort(Subsort);
            CurrentButtonEventContext.SetToggled(true);
        }

        void ActionSetTerrainBrushTool(BuildModeServer.TerrainModificationModes BrushMode)
        {
            ActionChangeTool(BuildTools.TerrainBrush);
            (BuildModeServer.Get().CurrentTool as TerrainBrushTool).CurrentBrushMode = BrushMode;
            ActionModifyTerrainBrushSize(TerrainBrushTool.TerrainBrushSizes.Small);
        }

        void ActionModifyTerrainBrushSize(TerrainBrushTool.TerrainBrushSizes Size)
        {
            EnsureGroupToggled(typeof(BuildHUD_TerrainBrushSizeButtons));
            UIButtonComponent button = rootElement.GetChildByID<UIButtonComponent>((uint)BuildHUD_TerrainBrushSizeButtons.SmallSizeButton);
            switch (Size)
            {
                case TerrainBrushTool.TerrainBrushSizes.Medium: // med
                    button = rootElement.GetChildByID<UIButtonComponent>((uint)BuildHUD_TerrainBrushSizeButtons.MedSizeButton);
                    break;
                case TerrainBrushTool.TerrainBrushSizes.Large: // lg
                    button = rootElement.GetChildByID<UIButtonComponent>((uint)BuildHUD_TerrainBrushSizeButtons.LgSizeButton);
                    break;
                default:
                    Size = TerrainBrushTool.TerrainBrushSizes.Small; break;
            }
            button.SetToggled(true);
            (BuildModeServer.Get().CurrentTool as TerrainBrushTool).BrushSize = (int)Size;
        }
    }
}

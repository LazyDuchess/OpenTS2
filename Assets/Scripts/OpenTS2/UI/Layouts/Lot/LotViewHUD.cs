using OpenTS2.Common;
using OpenTS2.Engine.Modes.Build;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts
{

    /// <summary>
    /// The HUD found when the user is playing in a lot
    /// </summary>
    public class LotViewHUD : UILayoutInstance
    {
        //CONSTANTS 
        public enum HUD_UILotModes
        {
            None,
            Live,
            Buy,
            Build,
            Memories,
            Settings
        }

        const uint WallViewSelectorGadgetID = 0x00004000;
        /// <summary>
        /// probably not necessary since it's the only control in this UIData but im gonna roll w it
        /// </summary>
        const uint LotViewPuckID = 0xF15C6B85;
        //Wall View button that invokes the Wall View Selector has 4 states for each wall view mode
        const uint FoldButton_WallsDown = 0x00001100, FoldButton_WallsCutaway = 0x00001101, 
            FoldButton_WallsUp = 0x00001102, FoldButton_Roof = 0x00001103;
        //Buttons inside the Wall View selector that change the Wall View Mode
        const uint Button_WallsDown = 0x00000001, Button_WallsCutaway = 0x00000002,
            Button_WallsUp = 0x00000003, Button_WallsRoof = 0x00000004;
        const uint BuildButton = 0x77D97B28, BuyButton = 0x67D97B2A, LiveButton = 0x37D97B25, SettingsButton = 0x67D97B2F;
        const uint FloorUpButton = 0x00002001, FloorDownButton = 0x00002000;

        //PRIVATE
        UIComponent WallsUpDownSelector;
        UIComponent rootElement => Components[0];

        HUD_UILotModes CurrentMode = HUD_UILotModes.None;

        //EVENTS
        public class ModeChangeEventArgs
        {
            public bool Allow = false;
            HUD_UILotModes RequestedMode;

            public ModeChangeEventArgs(HUD_UILotModes requestedMode)
            {
                RequestedMode = requestedMode;
            }
        }
        public event EventHandler<ModeChangeEventArgs> OnModeChangeRequested;
        public event EventHandler<HUD_UILotModes> OnModeChanged;

        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x4905F85B, 0xA99D8A11, TypeIDs.UI);
        public LotViewHUD() : this(MainCanvas)
        {
            
        }
        public LotViewHUD(Transform Canvas) : base(Canvas)
        {
            Init();
            WireUpUIEvents();
        }

        void Toggle(UIComponent comp) => comp.transform.gameObject.SetActive(!comp.transform.gameObject.activeSelf);

        /// <summary>
        /// Sets the hud elements to default settings for first run
        /// </summary>
        void Init()
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Stretch);
            var puck = root.GetChildByID(LotViewPuckID);
            puck.SetAnchor(UIComponent.AnchorType.BottomLeft); // set gizmo to botleft
            puck.transform.position = new Vector3(0, 200, 0);

            WallsUpDownSelector = root.GetChildByID(WallViewSelectorGadgetID, false);
            Toggle(WallsUpDownSelector);
        }

        /// <summary>
        /// Sets up all button click event messages
        /// </summary>
        void WireUpUIEvents()
        {
            void RigUpSequential(uint Base, int count, Action Callback)
            {
                if (count < 0) return; // :|
                if (Base + count > uint.MaxValue) throw new Exception($"UI Element ID overflow in WireUpUIEvents RigUpSeq method! {Base} + {count}");
                for (int i = 0; i < count; i++)
                {
                    var button = rootElement.GetChildByID<UIButtonComponent>((uint)(Base + i)); // downcast doesn't matter per above
                    button.OnClick += Callback;
                }                
            }
            //Rig up Wall View Mode tray thing
            RigUpSequential(FoldButton_WallsDown, 4, ShowHideWallViewOptions);
            var modeButton = rootElement.GetChildByID<UIButtonComponent>(BuildButton);
            modeButton.OnClick += delegate { SetMode(HUD_UILotModes.Build); }; // build mode button clicked

            //Wall view modes buttons
            var wallButton = rootElement.GetChildByID<UIButtonComponent>(Button_WallsDown);
            wallButton.OnClick += delegate { SetWallsCutawayMode(WallsMode.Down); };
            wallButton = rootElement.GetChildByID<UIButtonComponent>(Button_WallsCutaway);
            wallButton.OnClick += delegate { SetWallsCutawayMode(WallsMode.Cutaway); };
            wallButton = rootElement.GetChildByID<UIButtonComponent>(Button_WallsUp);
            wallButton.OnClick += delegate { SetWallsCutawayMode(WallsMode.Up); };
            wallButton = rootElement.GetChildByID<UIButtonComponent>(Button_WallsRoof);
            wallButton.OnClick += delegate { SetWallsCutawayMode(WallsMode.Roof); };

            //floor buttons
            var floorButton = rootElement.GetChildByID<UIButtonComponent>(FloorUpButton);
            floorButton.OnClick += delegate { SetFloorLevel(1); };
            floorButton = rootElement.GetChildByID<UIButtonComponent>(FloorDownButton);
            floorButton.OnClick += delegate { SetFloorLevel(-1); };
        }

        void ShowHideWallViewOptions()
        {
            Toggle(WallsUpDownSelector);
        }

        void SetWallsCutawayMode(WallsMode Mode)
        {
            var server = BuildModeServer.Get();
            var state = server.WorldState;
            server.WorldState = new WorldState(state.Level, Mode);
            server.InvalidateLotState();
            ShowHideWallViewOptions();
        }

        void SetFloorLevel(int Change)
        {
            var server = BuildModeServer.Get();
            var state = server.WorldState;
            int level = Math.Max(0,state.Level + Change); // TODO: Make this base floor value...
            server.WorldState = new WorldState(level, state.Walls);
            server.InvalidateLotState();
        }

        /// <summary>
        /// Sets the mode shown in the HUD
        /// <para>Note: If there are no subscribers to <see cref="OnModeChangeRequested"/>, the mode change will always be successful without verification.</para>
        /// </summary>
        /// <param name="Mode"></param>
        public void SetMode(HUD_UILotModes Mode)
        {
            if (OnModeChangeRequested != null)
            {
                var args = new ModeChangeEventArgs(Mode);
                OnModeChangeRequested(this, args);
                if (!args.Allow) return;
            }
            //Change allowed

            CurrentMode = Mode;
            var buildModeButton = rootElement.GetChildByID<UIButtonComponent>(BuildButton);
            buildModeButton.SetToggled(false);            

            switch(Mode)
            {
                case HUD_UILotModes.Build:
                    buildModeButton.SetToggled(true);
                    break;
            }

            OnModeChanged?.Invoke(this, Mode);
        }
    }
}

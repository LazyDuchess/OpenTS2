// JDrocks450 11-22-2023 on GitHub

using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files;
using OpenTS2.Scenes.Lot.State;
using OpenTS2.Scenes.Lot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static OpenTS2.Content.DBPF.LotObjectAsset;
using OpenTS2.Scenes.Lot.Extensions;
using OpenTS2.Engine.Modes.Build;
using OpenTS2.UI.Layouts;
using OpenTS2.Client;
using System.Linq;
using OpenTS2.UI.Layouts.Lot;
using UnityEditor.SearchService;
using OpenTS2.Engine.Modes.Build.Tools;

/// <summary>
/// Serves the behavior for the Build Mode Test scene
/// </summary>
public class BuildModeTest : MonoBehaviour
{    
    // Init
    LotLoad _loadedLotProvider;
    LotArchitecture _architectrue => _loadedLotProvider.architecture;
    TestLotViewCamera viewCamera;
    BuildModeServer buildServer;
    LotViewHUDController hudController;

    void ContentStartup(bool LoadGameContent = true)
    {
        EPManager.Get().InstalledProducts = 0x3EFFF;
        ContentLoading.LoadContentStartup();

        var settings = Settings.Get();
        settings.CustomContentEnabled = false;

        if (!LoadGameContent) return;

        // Load effects.
        EffectsManager.Get().Initialize();
        //load the game content
        ContentLoading.LoadGameContentSync();

        CatalogManager.Get().Initialize();
    }

    void Awake()
    {
        //Load content
        ContentStartup(true);
    }

    void Start()
    {        
        //load the default lot for now
        _loadedLotProvider = new LotLoad("N001", 80); 

        //create the build mode server
        buildServer = new BuildModeServer(_loadedLotProvider);
        buildServer.ChangeTool(BuildTools.None);

        //set the camera transform
        viewCamera = GameObject.Find("Main Camera").GetComponent<TestLotViewCamera>();

        //connect to the HUD controller
        hudController = GameObject.Find("Scene/LotViewUIController").GetComponent<LotViewHUDController>();
        hudController.Puck.SetMode(LotViewHUD.HUD_UILotModes.Build); // set puck display state to be Build mode
        hudController.Puck.OnModeChangeRequested += HUD_GameModeRequest;
    }

    private void HUD_GameModeRequest(object sender, LotViewHUD.ModeChangeEventArgs e)
    {
        e.Allow = false; // locked in Build Mode for the time being
    }

    // Update is called once per frame
    void Update()
    {
        HandleTool();
    }

    void HandleTool()
    {
        if (buildServer.CurrentTool == null) return;

        //set cursor position
        ReevaluateTerrainMesh();
        bool successful = viewCamera.TranslateScreen2WorldPos(_loadedLotProvider.Floor, out var terrainPosition, out int currentCursorFloor);
        if (!successful) return; // do not update the cursor when the mouse leaves the lot boundary. this prevents any events from firing with build tools
        var buildCursorPos = new Vector3((float)Math.Round(terrainPosition.x, 0), terrainPosition.y, (float)Math.Round(terrainPosition.z, 0));
        Vector2Int wallcurPos = new Vector2Int((int)buildCursorPos.x, (int)buildCursorPos.z); // flat grid position for wall tool        

        //context
        BuildToolContext context = new BuildToolContext()
        {
            Cursor3DWorldPosition = terrainPosition,
            WandPosition = buildCursorPos,
            GridPosition = wallcurPos,  
            CursorFloor = currentCursorFloor
        };

        buildServer.CurrentTool.OnToolUpdate(context); // mouse moved
        if (Input.GetMouseButtonDown(0)) // mouse left click start
        { // start drag
            buildServer.CurrentTool.OnToolStart(context);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            buildServer.CurrentTool.OnToolCancel("User cancelled using the ESC key.");
            return;
        }
    }    

    /// <summary>
    /// Resets the <see cref="_terrainCollider"/> -- this may be necessary when the terrain mesh is updated, for example
    /// </summary>
    void ReevaluateTerrainMesh()
    {
        var floorComp = _architectrue.GetComponentByType<LotFloorComponent>(LotArchitecture.ArchitectureGameObjectTypes.floor);
        for(int floor = 0; floor < _architectrue.BaseFloor + _architectrue.Elevation.Depth; floor++)
        {
            int actualFloor = _architectrue.BaseFloor + floor;
            List<Collider> collidersByFloor = new List<Collider>();
            if (actualFloor == 0)
            {
                var terrainObject = GameObject.Find("terrain/Terrain");
                collidersByFloor.Add(terrainObject.GetComponent<MeshCollider>());
            }
            if(floorComp.TryGetCollidersByFloor(floor, out var colliders))
                collidersByFloor.AddRange(colliders);
            viewCamera.SetCameraDetectionMeshes(actualFloor, collidersByFloor);
        }
    }
}

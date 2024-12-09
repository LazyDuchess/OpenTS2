using OpenTS2.Content.DBPF;
using OpenTS2.Engine.Modes.Build.Tools;
using OpenTS2.Scenes.Lot;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Scenes.Lot.Extensions;
using OpenTS2.Scenes.Lot.State;
using log4net.Core;
using UnityEngine.UIElements;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Diagnostic;

namespace OpenTS2.Engine.Modes.Build
{

    /// <summary>
    /// Controls the flow of changes to the lot architecture and serves the functionality for Build Mode
    /// </summary>
    internal class BuildModeServer : MonoBehaviour
    {
        /// <summary>
        /// Modes that walls can be created
        /// </summary>
        public enum WallCreationModes
        {
            /// <summary>
            /// A single normal wall
            /// </summary>
            Single,
            /// <summary>
            /// A square area of walls
            /// </summary>
            Room,
            /// <summary>
            /// A diagonally oriented square area of walls
            /// </summary>
            DiagnonalRoom
        }
        /// <summary>
        /// Modes that describe the modification being made to the terrain
        /// </summary>
        public enum TerrainModificationModes
        {
            /// <summary>
            /// No brush selected
            /// </summary>
            None,
            /// <summary>
            /// Raise the terrain brush
            /// </summary>
            Raise,
            /// <summary>
            /// Lower the terrain brush
            /// </summary>
            Lower,
            /// <summary>
            /// Smooth an area of terrain brush
            /// </summary>
            Smooth,
            /// <summary>
            /// Pond brush
            /// </summary>
            Water,
            /// <summary>
            /// Terrain paints brush
            /// </summary>
            Paint
        }

        //STATIC        
        private static BuildModeServer Current { get; set; }
        public static BuildModeServer Get() => Current;

        private static Dictionary<BuildTools, AbstractBuildTool> _tools;
        private ushort dryWallDesignPattern;
        /// <summary>
        /// The floor currently selected using the UI
        /// <para/>This also is the highest level shown in the lot geometry
        /// </summary>
        public int CurrentFloor => loadedLot.Floor;

        //PRIVATE
        private readonly StringBuilder lotHistory = new StringBuilder();

        private readonly LotLoad loadedLot;
        private LotArchitecture architecture => loadedLot.Architecture;
        private bool ConstrainedFloorElevation => CheatSystem.GetProperty("constrainfloorelevation").GetStringValue().ToLower() == "true";

        //PUBLIC
        /// <summary>
        /// The current <see cref="WorldState"/>
        /// <para>See: <see cref="InvalidateLotState"/></para>
        /// </summary>
        public WorldState WorldState
        {
            get => loadedLot.WorldState; set => loadedLot.WorldState = value;
        }
        /// <summary>
        /// After making changes to the <see cref="WorldState"/>, use this to apply the changes
        /// </summary>
        public void InvalidateLotState() => loadedLot.InvalidateState();
        /// <summary>
        /// The tool that the user is currently using (or the one the user more recently selected)
        /// </summary>
        public BuildTools SelectedTool { get; private set;} = BuildTools.None;
        /// <summary>
        /// The handler for the tool the user is currently using
        /// </summary>
        public AbstractBuildTool CurrentTool { get; private set; } = null;
        public int BaseFloor => loadedLot.BaseFloor;
        public int MaxFloor => loadedLot.MaxFloor;

        private void LogHistory(string Message, string Caption = default, string Sender = default)
        {
            if (string.IsNullOrWhiteSpace(Sender)) Sender = typeof(BuildModeServer).Name;
            var msgTxt = $"OpenTS2::{Sender} -> {(!string.IsNullOrWhiteSpace(Caption) ? $"({Caption})" : "")} {Message}";
            lotHistory.AppendLine(msgTxt);
            Debug.Log(msgTxt);
        }

        internal BuildModeServer(LotLoad LoadedLot)
        {
            Current = this;

            loadedLot = LoadedLot;

            SelectedTool = BuildTools.None;
            CurrentTool = null;

            MakeTools();
            Init();
        }

        void MakeTools()
        {
            _tools = new Dictionary<BuildTools, AbstractBuildTool>
            { // funfact: these are added in the order they were implemented .. incase you were wondering
                { BuildTools.Wall, new WallTool(this) },
                { BuildTools.Hand, new HandTool(loadedLot, architecture) },
                { BuildTools.TerrainBrush, new TerrainBrushTool(this) },
                { BuildTools.Floor, new FloorTool(this) },
                { BuildTools.Foundation, new FoundationTool(this) }
            };
        }

        void Init()
        {
            //set drywall design pattern (used so frequently we can save a little time frontloading it)
            architecture.EnsurePatternReferenced("blank", LotArchitecture.ArchitectureGameObjectTypes.wall, out dryWallDesignPattern, out _);
        }
        /// <summary>
        /// Changes the currently used tool by the User.
        /// <para/>Passing <see cref="BuildTools.None"/> will drop the current tool, if held.
        /// </summary>
        /// <param name="Tool"></param>
        public void ChangeTool(BuildTools Tool)
        {
            if (CurrentTool != null)
            { // A tool is currently being used.
                if (Tool == SelectedTool)
                    return; // the newly selected tool is the same as the one currently held, exit.
                CurrentTool.OnToolCancel(Tool != BuildTools.None ? "The user selected a different tool." : "The current tool has been dropped.");
                CurrentTool.SetActive(false);
            }
            switch(Tool)
            {
                default:                
                case BuildTools.None:                    
                    CurrentTool = null; break;
                case BuildTools.Foundation:
                case BuildTools.Hand:
                case BuildTools.Wall:
                case BuildTools.TerrainBrush:
                case BuildTools.Floor:
                    CurrentTool = _tools[Tool];
                    CurrentTool.SetActive(true);
                    LogHistory($"Selected tool: {Tool} (trans from: {SelectedTool})", "Change Tool");
                    break;
            }
            SelectedTool = Tool;
        }

        /// <summary>
        /// Lists all the points for wall segments in the area of walls provided
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="CreationOption"></param>
        /// <returns></returns>
        public static List<(Vector2Int A, Vector2Int B)> GetWallPoints(Vector2Int From, Vector2Int To, WallCreationModes CreationOption = WallCreationModes.Single)
        {
            List<(Vector2Int A, Vector2Int B)> walls = new List<(Vector2Int, Vector2Int)>();
            switch (CreationOption)
            {
                case WallCreationModes.Single:
                    walls.Add((From, To));
                    break;
                case WallCreationModes.Room:                    
                    walls.Add((From, new Vector2Int(To.x, From.y)));
                    walls.Add((new Vector2Int(To.x, From.y), To));
                    walls.Add((To, new Vector2Int(From.x, To.y)));
                    walls.Add((new Vector2Int(From.x, To.y), From));
                    break;
                case WallCreationModes.DiagnonalRoom: break;
            }
            return walls;
        }

        public float PollElevation(Vector2Int Position, int Floor) => architecture.PollElevation(Position, Floor);

        /// <summary>
        /// Creates walls with the blank wallpaper set with optional creation options like Square area of walls or Single
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Floor"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public int[]? CreateWalls(Vector2Int From, Vector2Int To, int Floor, 
            WallType Type = WallType.Normal, 
            WallCreationModes CreationOption = WallCreationModes.Single,
            string FrontWallpaperPattern = "blank", string BackWallpaperPattern = "blank")            
        {
            if (From == To) return null;
            if (CreationOption != WallCreationModes.Single && !RegionValid(From, To)) return null;
            string argsStr = $"From: {From} To: {To} Floor: {Floor} Type: {Type} Mode: {CreationOption}" +
                $"Front: {FrontWallpaperPattern} Back: {BackWallpaperPattern}";

            //OrderPoints(ref From, ref To);
            var walls = GetWallPoints(From, To, CreationOption);

            //WALLPAPERS
            ushort front = dryWallDesignPattern;
            ushort back = front;
            if (FrontWallpaperPattern != "blank")
                architecture.EnsurePatternReferenced(FrontWallpaperPattern, LotArchitecture.ArchitectureGameObjectTypes.wall, out front, out _);
            if (BackWallpaperPattern != "blank")
                architecture.EnsurePatternReferenced(BackWallpaperPattern, LotArchitecture.ArchitectureGameObjectTypes.wall, out back, out _);

            int[] createdWallIDs = null;
            foreach (var wall in walls)
                createdWallIDs = architecture.CreateWall(wall.A, wall.B, Floor, Type, front, back);

            if (createdWallIDs != null) // walls were placed here
            {                
                loadedLot.InvalidateWalls();
                LogHistory($"Created wall(s). {argsStr}", "Create Walls");
                return createdWallIDs;
            }
            else LogHistory($"Could not create wall(s). {argsStr}", "Create Walls");
            return null;
        }
        /// <summary>
        /// Causes the room detection system to re-evaluate the lot.
        /// </summary>
        public void SignalRoomsInvalidate() => loadedLot.EvaluateRoomMap();
        /// <summary>
        /// Clears out all walls in the specified region with the given wall creation options
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Floor"></param>
        /// <param name="Type"></param>
        /// <param name="CreationOption"></param>
        public void DeleteWalls(Vector2Int From, Vector2Int To, int Floor, WallCreationModes CreationOption = WallCreationModes.Single)            
        {
            var walls = GetWallPoints(From, To, CreationOption);
            string argsStr = $"From: {From} To: {To} Floor: {Floor} Mode: {CreationOption}";

            bool? returnValue = true;
            foreach (var wall in walls)
                returnValue = architecture.DeleteWall(wall.A, wall.B, Floor);

            if (returnValue.HasValue) // walls were placed here
            {
                loadedLot.InvalidateWalls();
                LogHistory($"Deleted wall(s). {argsStr}", "Delete Walls");
            }
            else LogHistory($"Could not delete wall(s). {argsStr}", "Delete Walls");
        }
        public void DeleteAllWalls(params int[] WallLayerIDs)
        {
            if (WallLayerIDs.Length == 0) return;
            architecture.DeleteAllWalls(WallLayerIDs);
            loadedLot.InvalidateWalls();
        }

        /// <summary>
        /// Puts the two points in ascending order
        /// </summary>
        void OrderPoints(ref Vector2Int A, ref Vector2Int B)
        {
            var a = new Vector2Int(Math.Min(A.x, B.x), Math.Min(A.y, B.y));
            var b = new Vector2Int(Math.Max(A.x, B.x), Math.Max(A.y, B.y));
            A = a;
            B = b;
        }

        /// <summary>
        /// Checks if the area between two points is nonzero
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        bool RegionValid(Vector2Int From, Vector2Int To) => Math.Abs(From.x - To.x) * Math.Abs(From.y - To.y) != 0;

        /// <summary>
        /// Checks to see if there is a floor at the given location.
        /// <para>If <paramref name="Level"/> is provided as <see langword="default"/>, it will check floors on ALL levels.</para>
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Level"></param>
        /// <returns></returns>
        public bool IsFloorAt(Vector2Int Position, int? Level = default)
        {
            bool inquire(int level, int index)
            {
                var value = architecture.FloorPatterns.Data[level][index];
                return value.w != 0 || value.x != 0 || value.y != 0 || value.z != 0;
            }            

            int level = 0;
            bool allLevels = Level == default;
            if (!allLevels)
                level = -architecture.BaseFloor + Level.Value;

            var mPos = Position;

            bool inquiryList(int level, int index)
            {
                if (inquire(level, index)) return true;
                if (index == 0) return false;
                if (inquire(level, index - 1)) return true;
                index = ((mPos.x - 1) * architecture.FloorPatterns.Height) + mPos.y;
                if (inquire(level, index)) return true;
                return false;
            }

            int index = (mPos.x * architecture.FloorPatterns.Height) + mPos.y;

            if (!allLevels)            
                return inquiryList(level, index);            

            for (level = architecture.BaseFloor; level < architecture.FloorPatterns.Data.Length; level++)            
                if (inquiryList(level, index)) return true;            
            return false;
        }

        /// <summary>
        /// Ensures the provided <see cref="_3DArrayView{T}"/> has layers up to the provided floor level
        /// </summary>
        /// <param name="FillFunc">Function used to fill the array's new level(s) if applicable. 
        /// <para/><c>arg1</c> is the index in the array being filled
        /// <para/><c>arg2</c> is the value at <c>arg1</c> on the floor beneath this one. If this is the lowest floor,
        /// it will reference itself.</param>
        void Ensure3DViewDepth2Floor<T>(_3DArrayView<T> ArrayView, int Floor, Func<int, T, T> FillFunc) where T : unmanaged
        {
            //ensure a layer for elevation exists
            if (ArrayView.Depth <= Floor)
            {
                int fromFloor = ArrayView.Depth;
                int toFloor = Floor;
                //make new level(s)
                ArrayView.Resize(Floor + 1);
                for(int f = fromFloor; f <= toFloor; f++)
                { // iterate over new level(s)
                    int underFloor = f - 1;
                    if (underFloor < BaseFloor) underFloor = BaseFloor;
                    for (int i = 0; i < ArrayView.Width * ArrayView.Height; i++)
                    {
                        T underValue = ArrayView.Data[underFloor][i];
                        ArrayView.Data[f][i] = FillFunc(i, underValue); // invoke fillfunc to fill the array
                    }
                }
                ArrayView.Commit();
            }
        }
        /// <summary>
        /// Ensures the provided <see cref="_3DArrayView{T}"/> has layers up to the provided floor level
        /// </summary>
        /// <param name="DefaultFillVal">Value used to fill the newly created array levels</param>
        void Ensure3DViewDepth2Floor<T>(_3DArrayView<T> ArrayView, int Floor, T DefaultFillVal) where T : unmanaged
        {
            //ensure a layer for elevation exists
            if (ArrayView.Depth <= Floor)
            {
                int fromFloor = ArrayView.Depth;
                int toFloor = Floor;
                //make new level(s)
                ArrayView.Resize(Floor + 1);
                for (int f = fromFloor; f <= toFloor; f++)
                { // iterate over new level(s)
                    for (int i = 0; i < ArrayView.Width * ArrayView.Height; i++)
                        ArrayView.Data[f][i] = DefaultFillVal; // invoke fillfunc to fill the array
                }
                ArrayView.Commit();
            }
        }

        public void DeleteFloors(Vector2Int From, Vector2Int To, int Level = 0) =>
            CreateFloors(From, To, 0, Level, false);
        /// <summary>
        /// Creates an area of flooring with the specified <paramref name="PatternName"/>.
        /// <para/>This function handles adding the pattern reference to the <see cref="LotArchitecture.FloorMap"/>
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="PatternName"></param>
        /// <param name="Level"></param>
        /// <param name="ModifyTerrain"></param>
        public void CreateFloors(Vector2Int From, Vector2Int To, string PatternName, int Level = 0, bool ModifyTerrain = true)
        {
            if (!architecture.EnsurePatternReferenced(PatternName, LotArchitecture.ArchitectureGameObjectTypes.floor, out ushort patternID, out _))
            {
                LogHistory($"Could not create flooring due to the pattern: {PatternName} not existing or isn't valid.");
                return;
            }    
            CreateFloors(From, To, patternID, Level, ModifyTerrain);
        }
        public void CreateFloors(Vector2Int From, Vector2Int To, ushort PatternID, int Level = 0, bool ModifyTerrain = true) =>
            CreateFloors(From, To, new Files.Formats.DBPF.Vector4<ushort>(PatternID, PatternID, PatternID, PatternID), Level, ModifyTerrain);
        public void CreateFloors(Vector2Int From, Vector2Int To,
            Vector4<ushort> QuarterTilePatternIDs, int Level = 0, bool ModifyTerrain = true)
        {
            if (!RegionValid(From, To)) return; // check area nonzero            

            var from = From; var to = To; // order the points in order of closest to origin of the lot first
            OrderPoints(ref from, ref to);

            if (ModifyTerrain) // flatten the area of terrain if applicable
            {
                Vector2Int pollPosition = From; // NE corner of the origin tile always
                if(From.y > To.y) // selection is towards NORTH                
                    pollPosition.y -= 1;
                else pollPosition.y += 1;
                if (From.x > To.x) // selection is towards WEST                
                    ;
                else pollPosition.x += 1;

                float elevation = architecture.PollElevation(pollPosition, Level);
                LevelRegion(from, to, elevation, Level); // level terrain under floors
            }
            Level = -architecture.BaseFloor + Level;
            _3DArrayView<Vector4<ushort>> floorPatterns = architecture.FloorPatterns;

            //ensure a layer for elevation exists
            Ensure3DViewDepth2Floor(floorPatterns, Level, new Vector4<ushort>()); // fill with empty

            ref Vector4<ushort>[] baseFloorData = ref floorPatterns.Data[Level];

            int changedFloors = 0;
            for (int tx = from.x; tx < to.x; tx++)
            {
                for (int ty = from.y; ty < to.y; ty++)
                {
                    var mPos = new Vector2Int(tx, ty);
                    int index = (mPos.x * floorPatterns.Height) + mPos.y;

                    if (index > baseFloorData.Length - 1)
                        index = baseFloorData.Length - 1;
                    if (index < 0)
                        index = 0;

                    baseFloorData[index] = QuarterTilePatternIDs;
                    changedFloors++;
                }
            }

            if (changedFloors <= 0) return;

            LogHistory($"Modified flooring. From: {From} To: {To} PatIDs: {QuarterTilePatternIDs}" +
                $" Floor: {Level} TerrainMod?: {ModifyTerrain}", "Modify Flooring");
            loadedLot.InvalidateFloors(); // invalidating floors also may incur a terrain re-evaluation if necessary
        }

        /// <summary>
        /// Levels a region of terrain/elevation map.
        /// </summary>
        /// <param name="From">The point to start at</param>
        /// <param name="To">The point to go to.</param>
        /// <param name="Elevation">What elevation value to level to.</param>
        /// <param name="Floor">Which level of the Elevation Map to perform the modification.</param>
        /// <param name="LevelFloorsBeneathMe">If true, will ensure each level beneath <paramref name="Floor"/> does not exceed this elevation.
        /// Only greater elevations will be modified, lower ones will be ignored.</param>
        /// <param name="LevelBeneathMeDelta">In the event the terrain on a lower floor exceeds the elevation here on this floor, by how much to 
        /// decrease <paramref name="Elevation"/> to yield the elevation of the terrain on the lower floor?</param>
        public void LevelRegion(Vector2Int From, Vector2Int To, float Elevation = 0, int Floor = 0, bool LevelFloorsBeneathMe = true, float LevelBeneathMeDelta = -0f)
        {
            if (!RegionValid(From, To)) return;
            Floor = -architecture.BaseFloor + Floor;

            var from = From; var to = To;
            OrderPoints(ref from, ref to);

            //check the cheat system for constrainfloorelevation cheat
            bool cfeProp = ConstrainedFloorElevation;

            _3DArrayView<float> arr = architecture.Elevation;

            //ensure a layer for elevation exists
            //fill function will set the entire level to appropriate wall height.
            Ensure3DViewDepth2Floor(arr, Floor, (int i, float beneathElevation) => beneathElevation + LotWallComponent.WallHeight);

            ref float[] dataSet = ref arr.Data[Floor];
            bool levelModify = false;
            for (int tx = from.x; tx <= to.x; tx++)
            {
                for(int ty = from.y; ty <= to.y; ty++)
                {                                 
                    var mPos = new Vector2Int(tx, ty);
                    int index = (mPos.x * arr.Height) + mPos.y;

                    if (index > dataSet.Length - 1)
                        index = dataSet.Length - 1;
                    if (index < 0)
                        index = 0;

                    if (cfeProp && IsFloorAt(mPos))
                        continue;

                    dataSet[index] = Elevation;
                    levelModify = true;

                    //level beneath me
                    if (LevelFloorsBeneathMe)
                    {
                        try
                        {
                            for (int elevationLevel = Floor; elevationLevel > 0; elevationLevel--)
                            {
                                ref float[] meValDataSet = ref arr.Data[elevationLevel - 1];
                                meValDataSet[index] = Math.Min(Elevation + LevelBeneathMeDelta, meValDataSet[index]);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"LevelRegion() :: LevelBeneathMe error: {e.Message}");
                        }
                    }
                }
            }

            if (!levelModify) return;

            LogHistory($"Leveled terrain. From: {From} To: {To} Elevation: {Elevation}" +
                $" Floor: {Floor}", "Level Terrain");
            //all changes complete, invalidate mesh
            loadedLot.InvalidateFloors();
        }
        /// <summary>
        /// Sets the elevation at the position in lot grid coordinates at the Floor provided by adding it to the elevation of the lower level.
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="RelativeElevation"></param>
        /// <param name="Floor"></param>
        public void SetElevationRelative(float RelativeElevation, int Floor, params Vector2Int[] Positions)
        {
            Floor = -architecture.BaseFloor + Floor;

            _3DArrayView<float> arr = architecture.Elevation;
            //ensure a layer for elevation exists
            //fill function will set the entire level to appropriate wall height.
            Ensure3DViewDepth2Floor(arr, Floor, (int i, float beneathElevation) => beneathElevation + RelativeElevation);

            ref float[] dataSet = ref arr.Data[Floor];
            foreach (var p in Positions)
            {
                var mPos = p;
                int index = (mPos.x * arr.Height) + mPos.y;

                float lowerElevation = dataSet[index];
                if (Floor > 0)
                    lowerElevation = arr.Data[Floor - 1][index];

                dataSet[index] = lowerElevation + RelativeElevation;
            }
        }

        /// <summary>
        /// Submits a terrain modification with the supplied parameters
        /// </summary>
        /// <param name="Position">The center point of the terrain modification, in lot grid coordinates</param>
        /// <param name="Radius">The radius of the terrain brush being used, in grid cells</param>
        /// <param name="Strength">By how much should the terrain be altered in this call</param>
        /// <param name="Mode">Which modification is requested</param>
        /// <param name="Floor"></param>
        public bool ModifyTerrain(Vector2Int Position, int Radius, float Strength, TerrainModificationModes Mode, int Floor = 0)
        {
            if (Mode == TerrainModificationModes.None) return false;
            Floor = -architecture.BaseFloor + Floor;
            ref float[] dataSet = ref architecture.Elevation.Data[Floor];

            var mPos = Position;
            int index = (mPos.x * architecture.Elevation.Height) + mPos.y;

            //The Sims 2 won't allow any terrain modifications if the cursor is directly over unexposed terrain
            if (ConstrainedFloorElevation && IsFloorAt(mPos, Floor))
                return false;

            bool levelModify = false;

            for (int dx = -Radius; dx <= Radius; dx++)
            {
                for (int dy = -Radius; dy <= Radius; dy++)
                {
                    //get position to edit
                    mPos = Position + new Vector2Int(dx, dy);
                    index = (mPos.x * architecture.Elevation.Height) + mPos.y;

                    //measure length from center to this point
                    int length = (int)Math.Round(Vector2.Distance(mPos, Position),0);
                    if (length > Radius) continue;

                    float _stren = Strength;

                    //CFE cheat
                    if (ConstrainedFloorElevation && IsFloorAt(mPos))
                        continue;

                    //terrain modification
                    switch (Mode)
                    {
                        default: return false; // ??
                        case TerrainModificationModes.Water:
                        case TerrainModificationModes.Lower:
                            _stren *= -1;
                            goto case TerrainModificationModes.Raise;
                        case TerrainModificationModes.Raise:
                            dataSet[index] += _stren;
                            levelModify = true;
                            break;
                    }
                }
            }

            if (!levelModify) return false;

            LogHistory($"{Mode}ed terrain. Center: {Position} Radius: {Radius} Strength: {Strength} Level: {Floor}", "Level Terrain");
            //all changes complete, invalidate mesh
            loadedLot.InvalidateFloors();
            return true;
        }

        /// <summary>
        /// Resets the lot to having nothing on it and flattened terrain
        /// <para/>Note: It is highly recommended to alert the user before performing an action like this.
        /// </summary>
        /// <param name="Elevation"></param>
        public void FlattenLot(float Elevation = 0)
        {
            int Floor = 0;
            Floor = -architecture.BaseFloor + Floor;
            ref float[] dataSet = ref architecture.Elevation.Data[-architecture.BaseFloor];
            for(int i = 0; i < dataSet.Length; i++)            
                dataSet[i] = Elevation;
            //all changes complete, invalidate mesh
            loadedLot.InvalidateFloors();
        }
    }
}

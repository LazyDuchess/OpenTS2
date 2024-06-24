using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot.Roof;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class LotWallComponent : AssetReferenceComponent, IPatternMaterialConfigurator
    {
        private const string ThicknessTexture = "wall_top";
        private const string FallbackMaterial = "wall_wallboard";
        private const float HalfWallHeight = 1f;
        public const float WallHeight = 3f;
        private const float WallsDownHeight = 0.2f;
        private const float YBias = 0.001f;
        private const float Thickness = 0.075f;
        private const float RoofOffset = Thickness * 2.5f;

        private static Dictionary<string, string> Builtins = new Dictionary<string, string>()
        {
            { "blank", FallbackMaterial },
            { "deckskirtminimal", null },
            { "foundationbrick", "wall_brick_base" },
            { "deckredwood", "wall_deckredwoodb_base" },
            { "wall_poolroundedlipblue", "wall_poolroundedlipblue_base" }
        };

        private struct WallIntersectionMember
        {
            public int WallID;
            public float LeftExtent;
            public float RightExtent;

            public WallIntersectionMember(int wallId)
            {
                WallID = wallId;
                LeftExtent = 0;
                RightExtent = 0;
            }
        }

        private class WallIntersection
        {
            public WallGraphPositionEntry Position;
            public List<WallIntersectionMember> IncomingLines;
            public bool Simple; // Wall intersection is flat or capped.

            public WallIntersection(WallGraphPositionEntry position)
            {
                Position = position;
                IncomingLines = new List<WallIntersectionMember>();
                Simple = true;
            }
        }

        private LotArchitecture _architecture;

        private PatternMeshCollection _patterns;
        private PatternDescriptor[] _loadedPatternDescriptions;

        private Dictionary<int, WallIntersection> _intersections;

        public bool ExtraUV => true;

        public LotWallComponent CreateFromLotArchitecture(LotArchitecture architecture)
        {
            _architecture = architecture;

            Invalidate();

            gameObject.transform.position = new Vector3(0, -YBias, 0);

            return this;
        }

        public override void Invalidate()
        {
            LoadPatterns();
            BuildWallIntersections();
            BuildWallMeshes();
            AddFencePosts();
        }

        private ScenegraphMaterialDefinitionAsset LoadMaterial(ContentProvider contentProvider, string name)
        {
            var material = contentProvider.GetAsset<ScenegraphMaterialDefinitionAsset>(new ResourceKey($"{name}_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXMT));

            AddReference(material);

            return material;
        }

        private void LoadPatterns()
        {
            // Load the patterns. Some references are by asset name (do not exist in catalog), others are by catalog GUID.

            var contentProvider = ContentProvider.Get();
            var catalogManager = CatalogManager.Get();
            var patternMap = _architecture.WallMap.Map;

            ushort highestId = patternMap.Count == 0 ? (ushort)0 : patternMap.Keys.Max();
            PatternDescriptor[] patterns = new PatternDescriptor[highestId + 2];

            bool changesMade = false;
            bool previousStateExisting = _loadedPatternDescriptions != null;

            foreach (StringMapEntry entry in patternMap.Values)
            {
                //Persistent data -- calls from Invalidate() will call this with data already loaded, in which case
                //We can save time by not reloading the same loaded data
                // TODO
                if (previousStateExisting && _loadedPatternDescriptions.Length == patterns.Length && 
                    _loadedPatternDescriptions[entry.Id + 1].Name != null) // loaded
                { // PatternDescriptor is a struct -- check the name see if it's null, if not, the pattern is loaded already
                    patterns[entry.Id + 1] = _loadedPatternDescriptions[entry.Id + 1];
                    continue;
                }
                changesMade = true;

                string materialName;
                if (uint.TryParse(entry.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint guid))
                {
                    var catalogEntry = catalogManager.GetEntryById(guid);

                    materialName = catalogEntry?.Material ?? FallbackMaterial;
                }
                else if (!Builtins.TryGetValue(entry.Value, out materialName))
                {
                    materialName = entry.Value.StartsWith("wall_") ? (entry.Value + "_base") : ("wall_" + entry.Value + "_base");
                }

                if (materialName == null)
                {
                    // Explicitly remove this pattern.
                    continue;
                }

                // Try fetch the texture using the material name.

                var material = LoadMaterial(contentProvider, materialName);

                try
                {
                    // Note: Sometimes walls use the standard material, but we want them to use a special shader for cutaways.
                    patterns[entry.Id + 1] = new PatternDescriptor(
                        materialName,
                        material == null ? null : material?.GetAsUnityMaterial("Wallpaper")
                    );
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (previousStateExisting)
            {
                if (!changesMade) return; // no changes to wall patterns since last load 

                patterns[0] = _loadedPatternDescriptions[0];
                _loadedPatternDescriptions = patterns;
                _patterns.UpdatePatterns(patterns);
                return;
            }

            patterns[0] = new PatternDescriptor(
                ThicknessTexture,
                LoadMaterial(contentProvider, ThicknessTexture).GetAsUnityMaterial()
            );

            _loadedPatternDescriptions = patterns;
            _patterns = new PatternMeshCollection(gameObject, patterns, Array.Empty<PatternVariant>(), this, _architecture.WallGraphAll.Floors);
        }

        private bool IsWallThick(in WallLayerEntry elem)
        {
            if (IsFence(elem.WallType))
            {
                return elem.Pattern1 != 65535 || elem.Pattern2 != 65535;
            }

            switch (elem.WallType)
            {
                case WallType.Normal:
                case WallType.Roof:
                case WallType.Foundation:
                case WallType.Pool:
                case WallType.OFBWall:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsDeck(WallType type)
        {
            switch (type)
            {
                case WallType.Deck:
                case WallType.Deck2:
                case WallType.Deck3:
                case WallType.DeckInvis:
                    return true;

                default:
                    return false;
            }
        }

        private bool IsFence(WallType type)
        {
            return (uint)type > 255;
        }

        private static int CalculateElevationIndex(int height, int x, int y)
        {
            return x * height + y;
        }

        private static float GetElevationInt(float[] elevation, int width, int height, float x, float y)
        {
            return elevation[CalculateElevationIndex(height, (int)x, (int)y)];
        }

        private static float GetElevationIntUpper(float[] elevation, int width, int height, float x, float y, float previous)
        {
            return elevation == null ? previous + WallHeight : elevation[CalculateElevationIndex(height, (int)x, (int)y)];
        }

        private static float GetElevationInterp(float[] elevation, int width, int height, float x, float y)
        {
            int wm1 = width - 1;
            int hm1 = height - 1;

            float i0 = elevation[CalculateElevationIndex(height, (int)x, (int)y)];
            float i1 = elevation[CalculateElevationIndex(height, Math.Min(wm1, (int)x + 1), (int)y)];
            float j0 = elevation[CalculateElevationIndex(height, (int)x, Math.Min(hm1, (int)y + 1))];
            float j1 = elevation[CalculateElevationIndex(height, Math.Min(wm1, (int)x + 1), Math.Min(hm1, (int)y + 1))];

            float xi = x % 1;
            float yi = y % 1;

            return Mathf.Lerp(Mathf.Lerp(i0, i1, xi), Mathf.Lerp(j0, j1, xi), yi);
        }

        private static float GetElevationInterpUpper(float[] elevation, int width, int height, float x, float y, float previous)
        {
            return elevation == null ? previous + WallHeight : GetElevationInterp(elevation, width, height, x, y);
        }

        private WallIntersection GetOrAddIntersection(WallGraphAsset wallGraph, int id)
        {
            if (_intersections.TryGetValue(id, out WallIntersection intersection))
            {
                return intersection;
            }

            intersection = new WallIntersection(wallGraph.Positions[id]);
            _intersections.Add(id, intersection);

            return intersection;
        }

        private static void AddToIntersection(WallGraphAsset wallGraph, WallIntersection intersection, ref WallGraphLineEntry line, int lineIndex, bool isTo)
        {
            WallIntersectionMember newMember = new WallIntersectionMember(lineIndex);

            if (intersection.IncomingLines.Count > 0)
            {
                // Evaluate extents for new line, update others based on preference.

                ref WallGraphPositionEntry pos = ref intersection.Position;
                WallGraphPositionEntry inFrom = isTo ? wallGraph.Positions[line.FromId] : wallGraph.Positions[line.ToId]; //y

                Vector2 vecIntoIntersection = new Vector2(pos.XPos - inFrom.XPos, pos.YPos - inFrom.YPos);
                vecIntoIntersection.Normalize();

                int count = intersection.IncomingLines.Count;

                newMember.LeftExtent = float.PositiveInfinity;
                newMember.RightExtent = float.PositiveInfinity;

                for (int i = 0; i < count; i++)
                {
                    WallIntersectionMember otherMember = intersection.IncomingLines[i];

                    ref WallGraphLineEntry otherLine = ref wallGraph.Lines[otherMember.WallID];
                    bool otherTo = otherLine.FromId == intersection.Position.Id;
                    WallGraphPositionEntry outTo = otherTo ?
                        wallGraph.Positions[otherLine.ToId] :
                        wallGraph.Positions[otherLine.FromId];

                    Vector2 vecOutIntersection = new Vector2(outTo.XPos - pos.XPos, outTo.YPos - pos.YPos);

                    vecOutIntersection.Normalize();

                    float dot = Vector2.Dot(vecIntoIntersection, vecOutIntersection);

                    if (Mathf.Approximately(dot, 1f) && count == 1)
                    {
                        // If the two lines are facing the same direction, the extents are still 0 and this is still the simple case.

                        newMember.LeftExtent = 0;
                        newMember.RightExtent = 0;

                        break;
                    }

                    float vecAngle = Mathf.Acos(dot) * Mathf.Sign(vecIntoIntersection.x * vecOutIntersection.y - vecIntoIntersection.y * vecOutIntersection.x);
                    float angle = (vecAngle) / 2;

                    float extent = Thickness * Mathf.Tan(angle);

                    // When incoming, the left side extends forwards by extent, right side extends forwards by -extent.
                    // When outgoing, it's the opposite, as an incoming line's left side is the right side of the outgoing version.
                    // Prefer the smallest extent between both lines in the intersection.

                    float newSign = isTo ? 1 : -1;
                    newMember.LeftExtent = Math.Min(extent * newSign, newMember.LeftExtent);
                    newMember.RightExtent = Math.Min(-extent * newSign, newMember.RightExtent);

                    if (count == 1)
                    {
                        // Prepare these for getting new extents. Only saved back in non-simple-cases.
                        otherMember.LeftExtent = float.PositiveInfinity;
                        otherMember.RightExtent = float.PositiveInfinity;
                    }

                    float otherSign = otherTo ? 1 : -1;

                    otherMember.LeftExtent = Math.Min(extent * otherSign, otherMember.LeftExtent);
                    otherMember.RightExtent = Math.Min(-extent * otherSign, otherMember.RightExtent);

                    // Save back new extents.
                    intersection.IncomingLines[i] = otherMember;
                }
            }

            intersection.IncomingLines.Add(newMember);
        }

        private void BuildWallIntersections()
        {
            WallGraphLineEntry[] lines = _architecture.WallGraphAll.Lines;
            _intersections = new Dictionary<int, WallIntersection>();

            Dictionary<int, WallLayerEntry> wallLayer = _architecture.WallLayer.Walls;
            WallGraphAsset wallGraph = _architecture.WallGraphAll;

            for (int i = 0; i < lines.Length; i++)
            {
                ref WallGraphLineEntry line = ref lines[i];
                WallIntersection from = GetOrAddIntersection(wallGraph, line.FromId);
                WallIntersection to = GetOrAddIntersection(wallGraph, line.ToId);

                if (wallLayer.TryGetValue(line.LayerId, out var layer) && IsWallThick(layer))
                {
                    // Add this line to both intersections
                    AddToIntersection(wallGraph, from, ref line, i, false);
                    AddToIntersection(wallGraph, to, ref line, i, true);
                }
            }
        }

        private void BuildWallMeshes()
        {
            // TODO: split pattern meshes by level?
            _3DArrayView<float> elevationData = _architecture.Elevation;
            RoofCollection roofs = _architecture.Roof;

            int width = elevationData.Width;
            int height = elevationData.Height;
            int floors = elevationData.Depth;
            int baseFloor = _architecture.BaseFloor;

            _patterns.ClearAll();

            WallGraphLineEntry[] lines = _architecture.WallGraphAll.Lines;
            Dictionary<int, WallGraphPositionEntry> positions = _architecture.WallGraphAll.Positions;
            Dictionary<int, WallLayerEntry> layer = _architecture.WallLayer.Walls;

            // Draw each line on the wall graph.

            Vector3[] wallVertices = new Vector3[6];
            Vector3[] wallVertices2 = new Vector3[6];
            Vector3[] endVertices = new Vector3[4];

            Vector2[] blankWallsDown = new Vector2[6];

            Vector2[] endUVs = new Vector2[4];
            Vector2[] wallUVs = new Vector2[6];
            Vector2[] endWallsDown = new Vector2[4];
            Vector2[] wallsDown = new Vector2[6];

            Vector3[] thicknessVerts = new Vector3[8];
            Vector2[] thicknessUVs = new Vector2[8];
            Vector2[] thicknessWallsDown = new Vector2[8];

            int currentFloor = -1;
            float[] currentFloorElevation = null;
            float[] nextFloorElevation = null;

            LotArchitectureMeshComponent thicknessComp = null;
            PatternMeshFloor meshFloor = null;

            for (int i = 0; i < lines.Length; i++)
            {
                ref WallGraphLineEntry line = ref lines[i];

                WallIntersection fromI = _intersections[line.FromId];
                WallIntersection toI = _intersections[line.ToId];

                ref WallGraphPositionEntry from = ref fromI.Position;
                ref WallGraphPositionEntry to = ref toI.Position;

                int floor = from.Level - baseFloor;

                if (floor != currentFloor)
                {
                    currentFloor = floor;

                    currentFloorElevation = elevationData.Data[floor];
                    nextFloorElevation = floor + 1 < floors ? elevationData.Data[floor + 1] : null;

                    meshFloor = _patterns.GetFloor(floor);
                    thicknessComp = meshFloor.Get(0).Component;
                }

                if (!layer.TryGetValue(line.LayerId, out WallLayerEntry layerElem))
                {
                    layerElem = new WallLayerEntry()
                    {
                        Id = line.LayerId,
                        Pattern1 = 65535,
                        Pattern2 = 65535,
                        WallType = WallType.ThinFence
                    };

                    // This shouldn't happen...
                }

                bool isHalfWall = false;
                
                if (IsFence(layerElem.WallType))
                {
                    var fence = meshFloor.GetFence((uint)layerElem.WallType);

                    fence.AddRail(
                        from.XPos,
                        from.YPos,
                        to.XPos,
                        to.YPos,
                        GetElevationInt(currentFloorElevation, width, height, from.XPos, from.YPos),
                        GetElevationInt(currentFloorElevation, width, height, to.XPos, to.YPos));

                    if (layerElem.Pattern1 == 65535 && layerElem.Pattern2 == 65535)
                    {
                        continue;
                    }
                    else
                    {
                        isHalfWall = true;
                    }
                }

                LotArchitectureMeshComponent lPattern = (layerElem.Pattern1 == 65535 ? null : meshFloor.Get(layerElem.Pattern1 + 1)?.Component);// ?? _thickness?.Component;
                LotArchitectureMeshComponent rPattern = (layerElem.Pattern2 == 65535 ? null : meshFloor.Get(layerElem.Pattern2 + 1)?.Component);// ?? _thickness?.Component;

                float midX = (from.XPos + to.XPos) / 2;
                float midY = (from.YPos + to.YPos) / 2;

                float floorFrom = GetElevationInt(currentFloorElevation, width, height, from.XPos, from.YPos);
                float floorMid = GetElevationInterp(currentFloorElevation, width, height, midX, midY);
                float floorTo = GetElevationInt(currentFloorElevation, width, height, to.XPos, to.YPos);

                wallVertices[0] = new Vector3(from.XPos, floorFrom, from.YPos);
                wallVertices[1] = new Vector3(midX, floorMid, midY);
                wallVertices[2] = new Vector3(to.XPos, floorTo, to.YPos);

                float heightTo = GetElevationIntUpper(nextFloorElevation, width, height, to.XPos, to.YPos, wallVertices[2].y);
                float heightMid = GetElevationInterpUpper(nextFloorElevation, width, height, midX, midY, wallVertices[1].y);
                float heightFrom = GetElevationIntUpper(nextFloorElevation, width, height, from.XPos, from.YPos, wallVertices[0].y);

                bool roofWall = layerElem.WallType == WallType.Roof;

                if (roofWall)
                {
                    heightTo = Mathf.Max(roofs.GetHeightAt(to.XPos, to.YPos, from.Level, heightTo, RoofOffset), floorTo);
                    heightMid = Mathf.Max(roofs.GetHeightAt(midX, midY, from.Level, heightMid, RoofOffset), floorMid);
                    heightFrom = Mathf.Max(roofs.GetHeightAt(from.XPos, from.YPos, from.Level, heightFrom, RoofOffset), floorFrom);
                }
                else if (isHalfWall)
                {
                    heightTo = Mathf.Min(heightTo, floorTo + HalfWallHeight);
                    heightMid = Mathf.Min(heightMid, floorMid + HalfWallHeight);
                    heightFrom = Mathf.Min(heightFrom, floorFrom + HalfWallHeight);
                }

                wallVertices[3] = new Vector3(to.XPos, heightTo, to.YPos);
                wallVertices[4] = new Vector3(midX, heightMid, midY);
                wallVertices[5] = new Vector3(from.XPos, heightFrom, from.YPos);

                float startUV = (wallVertices[5].y - wallVertices[0].y) / WallHeight;
                float midUV = (wallVertices[4].y - wallVertices[1].y) / WallHeight;
                float endUV = (wallVertices[3].y - wallVertices[2].y) / WallHeight;

                bool bottomUVs = roofWall || isHalfWall;

                wallUVs[0] = new Vector2(0, bottomUVs ? 1 : startUV);
                wallUVs[1] = new Vector2(0.5f, bottomUVs ? 1 : midUV);
                wallUVs[2] = new Vector2(1, bottomUVs ? 1 : endUV);
                wallUVs[3] = new Vector2(1, bottomUVs ? (1 - endUV) : 0);
                wallUVs[4] = new Vector2(0.5f, bottomUVs ? (1 - midUV) : 0);
                wallUVs[5] = new Vector2(0, bottomUVs ? (1 - startUV) : 0);

                wallsDown[3] = new Vector2(0, (wallVertices[3].y - wallVertices[2].y) - WallsDownHeight);
                wallsDown[4] = new Vector2(0, (wallVertices[4].y - wallVertices[1].y) - WallsDownHeight);
                wallsDown[5] = new Vector2(0, (wallVertices[5].y - wallVertices[0].y) - WallsDownHeight);

                bool isThick = IsWallThick(layerElem);

                if (!isThick && IsDeck(layerElem.WallType))
                {
                    float floorThickness = LotFloorComponent.Thickness - YBias;
                    float floorThicknessUV = floorThickness / WallHeight;

                    wallVertices[3].y -= floorThickness;
                    wallVertices[4].y -= floorThickness;
                    wallVertices[5].y -= floorThickness;

                    wallUVs[3].y += floorThicknessUV;
                    wallUVs[4].y += floorThicknessUV;
                    wallUVs[5].y += floorThicknessUV;
                }

                if (isThick)
                {
                    // Thick wall. Offset vertices and evaluate 

                    var wallVec = new Vector2(to.XPos - from.XPos, to.YPos - from.YPos);

                    wallVec.Normalize();
                    var thickVec = wallVec * Thickness;

                    var offsetR = new Vector3(-thickVec.y, 0, thickVec.x);
                    var offsetL = new Vector3(thickVec.y, 0, -thickVec.x);

                    WallIntersectionMember fromMember = fromI.IncomingLines.First(x => x.WallID == i);
                    WallIntersectionMember toMember = toI.IncomingLines.First(x => x.WallID == i);

                    var offsetLStart = offsetL + new Vector3(wallVec.x * fromMember.LeftExtent * -1, 0, wallVec.y * fromMember.LeftExtent * -1);
                    var offsetRStart = offsetR + new Vector3(wallVec.x * fromMember.RightExtent * -1, 0, wallVec.y * fromMember.RightExtent * -1);

                    var offsetLEnd = offsetL + new Vector3(wallVec.x * toMember.LeftExtent, 0, wallVec.y * toMember.LeftExtent);
                    var offsetREnd = offsetR + new Vector3(wallVec.x * toMember.RightExtent, 0, wallVec.y * toMember.RightExtent);

                    thicknessVerts[6] = wallVertices[5]; // Center Start
                    thicknessVerts[7] = wallVertices[3]; // Center End
                    
                    wallVertices2[0] = wallVertices[0] + offsetRStart;
                    wallVertices2[1] = wallVertices[1] + offsetR;
                    wallVertices2[2] = wallVertices[2] + offsetREnd;
                    wallVertices2[3] = wallVertices[3] + offsetREnd;
                    wallVertices2[4] = wallVertices[4] + offsetR;
                    wallVertices2[5] = wallVertices[5] + offsetRStart;

                    wallVertices[0] += offsetLStart;
                    wallVertices[1] += offsetL;
                    wallVertices[2] += offsetLEnd;
                    wallVertices[3] += offsetLEnd;
                    wallVertices[4] += offsetL;
                    wallVertices[5] += offsetLStart;

                    // Add wall thickness
                    
                    thicknessVerts[0] = wallVertices[5]; // L top start
                    thicknessVerts[1] = wallVertices[4]; // L top mid
                    thicknessVerts[2] = wallVertices[3]; // L top end
                    thicknessVerts[3] = wallVertices2[3]; // R top end
                    thicknessVerts[4] = wallVertices2[4]; // R top mid
                    thicknessVerts[5] = wallVertices2[5]; // R top start

                    for (int j = 0; j < 8; j++)
                    {
                        thicknessUVs[j] = new Vector2(0, thicknessVerts[j].z);
                    }

                    thicknessWallsDown[0] = wallsDown[5];
                    thicknessWallsDown[1] = wallsDown[4];
                    thicknessWallsDown[2] = wallsDown[3];
                    thicknessWallsDown[3] = wallsDown[3];
                    thicknessWallsDown[4] = wallsDown[4];
                    thicknessWallsDown[5] = wallsDown[5];
                    thicknessWallsDown[6] = wallsDown[5];
                    thicknessWallsDown[7] = wallsDown[3];

                    int thicknessBase = thicknessComp.GetVertexIndex();
                    thicknessComp.AddVertices(thicknessVerts, thicknessUVs, thicknessWallsDown);

                    // Start to Mid
                    thicknessComp.AddTriangle(thicknessBase, 1, 0, 4);
                    thicknessComp.AddTriangle(thicknessBase, 5, 4, 0);

                    // Mid to End
                    thicknessComp.AddTriangle(thicknessBase, 2, 1, 3);
                    thicknessComp.AddTriangle(thicknessBase, 4, 3, 1);

                    // Ends
                    thicknessComp.AddTriangle(thicknessBase, 7, 2, 3);
                    thicknessComp.AddTriangle(thicknessBase, 6, 5, 0);
                }

                if (lPattern != null)
                {
                    var lVertStart = lPattern.GetVertexIndex();
                    lPattern.AddVertices(wallVertices, wallUVs, isThick ? wallsDown : blankWallsDown);

                    // Start to Mid
                    lPattern.AddTriangle(lVertStart, 0, 5, 4);
                    lPattern.AddTriangle(lVertStart, 4, 1, 0);

                    // Mid to End
                    lPattern.AddTriangle(lVertStart, 1, 4, 3);
                    lPattern.AddTriangle(lVertStart, 3, 2, 1);
                }

                if (rPattern != null)
                {
                    wallUVs[0].x = 1;
                    wallUVs[2].x = 0;
                    wallUVs[3].x = 0;
                    wallUVs[5].x = 1;

                    var rVertStart = rPattern.GetVertexIndex();
                    rPattern.AddVertices(isThick ? wallVertices2 : wallVertices, wallUVs, isThick ? wallsDown : blankWallsDown);

                    // Start to Mid
                    rPattern.AddTriangle(rVertStart, 0, 1, 4);
                    rPattern.AddTriangle(rVertStart, 4, 5, 0);

                    // Mid to End
                    rPattern.AddTriangle(rVertStart, 1, 2, 3);
                    rPattern.AddTriangle(rVertStart, 3, 4, 1);
                }

                // Cap off wall ends.
                if (isThick)
                {
                    if (toI.Simple && toI.IncomingLines.Count == 1 && rPattern != null)
                    {
                        float bottomV = (wallVertices[3].y - wallVertices[2].y) / WallHeight;

                        endVertices[0] = wallVertices[2];
                        endVertices[1] = wallVertices2[2];
                        endVertices[2] = wallVertices2[3];
                        endVertices[3] = wallVertices[3];

                        endUVs[0] = new Vector2(1 - Thickness * 2, bottomV);
                        endUVs[1] = new Vector2(1, bottomV);
                        endUVs[2] = new Vector2(1, 0);
                        endUVs[3] = new Vector2(1 - Thickness * 2, 0);

                        endWallsDown[2] = wallsDown[3];
                        endWallsDown[3] = wallsDown[3];

                        var rVertStart = rPattern.GetVertexIndex();
                        rPattern.AddVertices(endVertices, endUVs, endWallsDown);

                        rPattern.AddTriangle(rVertStart, 0, 3, 2);
                        rPattern.AddTriangle(rVertStart, 2, 1, 0);
                    }

                    if (fromI.Simple && fromI.IncomingLines.Count == 1 && lPattern != null)
                    {
                        float bottomV = (wallVertices[5].y - wallVertices[0].y) / WallHeight;

                        endVertices[0] = wallVertices2[0];
                        endVertices[1] = wallVertices[0];
                        endVertices[2] = wallVertices[5];
                        endVertices[3] = wallVertices2[5];

                        endUVs[0] = new Vector2(1 - Thickness * 2, bottomV);
                        endUVs[1] = new Vector2(1, bottomV);
                        endUVs[2] = new Vector2(1, 0);
                        endUVs[3] = new Vector2(1 - Thickness * 2, 0);

                        endWallsDown[2] = wallsDown[5];
                        endWallsDown[3] = wallsDown[5];

                        var lVertStart = lPattern.GetVertexIndex();
                        lPattern.AddVertices(endVertices, endUVs, endWallsDown);

                        lPattern.AddTriangle(lVertStart, 0, 3, 2);
                        lPattern.AddTriangle(lVertStart, 2, 1, 0);
                    }
                }
            }

            _patterns.CommitAll();
        }

        private void AddFencePosts()
        {
            _3DArrayView<float> elevationData = _architecture.Elevation;

            int width = elevationData.Width;
            int height = elevationData.Height;
            int floors = elevationData.Depth;
            int baseFloor = _architecture.BaseFloor;

            FencePost[] entries = _architecture.FencePostLayer.Entries;

            PatternMeshFloor meshFloor = null;
            FenceCollection fence = null;
            int previousFloor = -1;
            uint lastGUID = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                ref FencePost entry = ref entries[i];

                int floor = entry.Level - baseFloor;

                if (floor != previousFloor)
                {
                    previousFloor = floor;

                    meshFloor = _patterns.GetFloor(floor);

                    lastGUID = 0;
                }

                if (entry.GUID != lastGUID)
                {
                    lastGUID = entry.GUID;
                    fence = meshFloor.GetFence(entry.GUID);
                }

                fence.AddPost(entry.XPos, entry.YPos, GetElevationInt(elevationData.Data[floor], width, height, entry.XPos, entry.YPos));
            }
        }

        public void UpdateDisplay(WorldState state)
        {
            _patterns.UpdateDisplay(state, _architecture.BaseFloor, DisplayUpdateType.Wall);
        }

        public void Configure(WorldState state, bool visible, bool isTop, Material mat)
        {
            bool areWallsDown = isTop && state.Walls <= WallsMode.Cutaway;

            mat.SetTexture("_WallsDownTex", areWallsDown ? Texture2D.whiteTexture : Texture2D.blackTexture);
            mat.SetVector("_InvLotSize", new Vector4(1f / _architecture.FloorPatterns.Width, 1f / _architecture.FloorPatterns.Height, 0, 0));
        }

        public void AlterMeshBounds(Mesh mesh)
        {
            Bounds bounds = mesh.bounds;
            bounds.Encapsulate(bounds.max - new Vector3(0, WallHeight, 0));
            mesh.bounds = bounds;
        }
    }
}
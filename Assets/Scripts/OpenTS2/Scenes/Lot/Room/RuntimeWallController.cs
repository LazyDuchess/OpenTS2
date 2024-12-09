// JDrocks450 11-22-2023 on GitHub

#define NEW_INVALIDATE
#undef NEW_INVALIDATE

using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    /// <summary>
    /// BETA implementation. 
    /// Needs a lot of work to be shippable.
    /// Will need to revisit this concept.
    /// </summary>
    internal class RuntimeWallController
    {
        [Serializable]
        public struct GraphPlot
        {
            public GraphPlot(float x, float y)
            {
                X = x;
                Y = y;
            }
            public float X { get; set; }
            public float Y { get; set; }
        }
        [Serializable]
        public struct GraphLine
        {
            public GraphLine(int fromID, int toID)
            {
                FromPlotID = fromID;
                ToPlotID = toID;
            }

            /// <summary>
            /// The <see cref="GraphPlot"/> this line originates
            /// </summary>
            public int FromPlotID { get; set; }
            /// <summary>
            /// The <see cref="GraphPlot"/> this line ends at
            /// </summary>
            public int ToPlotID { get; set; }

            public override string ToString() => $"({FromPlotID} -> {ToPlotID})";
        }

        /// <summary>
        /// A map of all positions in the WallGraph
        /// <para/>For only points that are on line segments, see: <see cref="JointPositions"/>
        /// </summary>
        public Dictionary<int, GraphPlot> Positions { get; private set; } = new Dictionary<int, GraphPlot>();
        /// <summary>
        /// <see cref="Positions"/> that are at the corners of two connected line segments
        /// </summary>
        public HashSet<int> JointPositions { get; private set; } = new HashSet<int>();
        /// <summary>
        /// Lines between <see cref="JointPositions"/> that form a straight wall of at least 1 unit length
        /// </summary>
        public Dictionary<int, GraphLine> Lines { get; private set; } = new Dictionary<int, GraphLine>();
        /// <summary>
        /// Groups of <see cref="Lines"/> that all form one enclosed shape (Room) on the lineplot
        /// </summary>
        public Dictionary<int, IEnumerable<int>> Rooms { get; private set; } = new Dictionary<int, IEnumerable<int>>();

        /// <summary>
        /// A serialized RoomState -- the current state of the walls on the lot
        /// </summary>
        [Serializable]
        public class RoomPackage
        {
            public Dictionary<int, GraphPlot> Positions;
            public HashSet<int> JointPositions;
            public Dictionary<int, GraphLine> Lines;
            public Dictionary<int, IEnumerable<int>> Rooms;
            public int Width, Height;
        }
        /// <summary>
        /// Simple distance formula, calculates the distance in units between two coordinates
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        double Distance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }
        /// <summary>
        /// Determines if three points create a triangle, can be used to detect straight walls versus joints
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        bool IsTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // Calculate the lengths of the sides
            double a = Distance(p1, p2);
            double b = Distance(p2, p3);
            double c = Distance(p3, p1);

            // Check if the triangle inequality theorem holds for all sides
            return a + b > c && b + c > a && c + a > b;
        }
        /// <summary>
        /// Regenerates the room map by discarding the previous data
        /// </summary>
        /// <param name="architecture"></param>
        public void GenerateAll(LotArchitecture architecture)
        {
            var graph = architecture.WallGraphAll;
            Dictionary<int, WallGraphPositionEntry> positionsData = graph.Positions;
            WallGraphLineEntry[] wallData = graph.Lines;

            if (!wallData.Any()) return;
            if (!positionsData.Any()) return;

            System.Diagnostics.Stopwatch debugTimer = new System.Diagnostics.Stopwatch();
            debugTimer.Start();

            GenerateLineGraph(positionsData, wallData);
            long genLineGraph = debugTimer.ElapsedMilliseconds;
            debugTimer.Restart();

            GenerateRoomMap();
            long genRoomMap = debugTimer.ElapsedMilliseconds;
            debugTimer.Restart();

            PackageRoomMap(graph.Width, graph.Height);
            long genPkg = debugTimer.ElapsedMilliseconds;
            debugTimer.Stop();

            long tTime = genLineGraph + genRoomMap + genPkg;

            UnityEngine.Debug.Log($"RoomMap(): Lines took {genLineGraph}ms, Rooms took {genRoomMap}ms, Package took {genPkg}ms" +
                $" and altogether took {tTime}ms. ");
            //GenerateImage(positionsData, graph.Width, graph.Height);
        }

        private void PackageRoomMap(int Width, int Height)
        {
            //Serialize this to file
            using (StringWriter strJson = new StringWriter())
            {
                JsonSerializer.CreateDefault().Serialize(strJson, new RoomPackage()
                {
                    JointPositions = JointPositions,
                    Lines = Lines,
                    Positions = Positions,
                    Rooms = Rooms,
                    Width = Width,
                    Height = Height
                }, typeof(RoomPackage));
                File.WriteAllText("C:\\Users\\xXJDr\\OneDrive\\Desktop\\roommapts2.txt", strJson.ToString());
            }            
        }

        /// <summary>
        /// This function takes the wall graph from package and processes it into a lineplot
        /// of only significant points -- as in points that form corners to build shapes.
        /// This function will also create the network of lines between the points.
        /// </summary>
        /// <param name="positionsData"></param>
        /// <param name="wallData"></param>
        private void GenerateLineGraph(Dictionary<int, WallGraphPositionEntry> positionsData, WallGraphLineEntry[] wallData)
        {                       
            //build a map of points that are referenced by lines.
            //Position ID to List of LineIDs in WallGraph
            Dictionary<int, HashSet<int>> PositionIDReferences = new Dictionary<int, HashSet<int>>();

            void AddPositionReference(int PositionID, int LayerID)
            {
                //Check if the point exists in our wall graph
                if (!positionsData.ContainsKey(PositionID)) return;
                //position existing
                if (PositionIDReferences.TryGetValue(PositionID, out HashSet<int> references))
                    references.Add(LayerID);
                else
                {
                    PositionIDReferences.Add(PositionID, new HashSet<int>() { LayerID });
                    if (!Positions.ContainsKey(PositionID))
                        Positions.Add(PositionID, new GraphPlot(positionsData[PositionID].XPos,
                            positionsData[PositionID].YPos));
                }
            }

            //find all points that are referenced by wall lines
            foreach (var wall in wallData)
            {
                AddPositionReference(wall.FromId, wall.LayerId);
                AddPositionReference(wall.ToId, wall.LayerId);
            }

            //map lines to their layerIds
            Dictionary<int, WallGraphLineEntry> lineMap = new Dictionary<int, WallGraphLineEntry>();
            foreach (var line in wallData)
                lineMap.Add(line.LayerId, line);

            List<int> processed = new List<int>();
            HashSet<int> WallLayerIDsOnLine = new HashSet<int>();
            Lines.Clear();

            //filter out positions that form straight lines to get a 
            //lineplot of all significant points -- e.g. the ones that form
            //corners
            //--
            //This is done by taking two connected wall lines and seeing if all three points
            //on these two lines form a triangle with any area -- if they do, it's a bend
            foreach (var positionItem in PositionIDReferences)
            {
                if (positionItem.Value.Count <= 1) // 1 or less connections can never form a wall
                    continue; // this is due to them being open-ended
                else if (positionItem.Value.Count == 2 && processed.Contains(positionItem.Key))
                    continue; // only two connections means it's already been processed and cannot be anything more

                int lineFrom = -1, lineTo = -1;

                //tunnelling function -- watch out for stack overflow by using cautiously
                //keep scope to only one level on the lot at a time, and this function tree will 
                //end as soon as a line segment is found
                // -- todo: set limit on tunnelling level to be diagonal line from lot corner to corner
                void Try(int meID, int tunneled)
                {
                    if (JointPositions.Contains(meID))
                    { // already a joint -- don't bother with the detection
                        lineTo = meID;
                        return;
                    }
                    var references = PositionIDReferences[meID]; // find all wall lines that contain this point
                    if (references.Count <= 1) goto exit; // none references!!
                    if (references.Count > 2)
                    {
                        JointPositions.Add(meID);
                        lineTo = meID;
                        if (tunneled == 0)
                            return;
                    }
                    //very bad -- change ASAP
                    WallGraphLineEntry line1 = lineMap[references.ElementAt(0)];
                    WallGraphLineEntry line2 = lineMap[references.ElementAt(1)];
                    int other1 = line1.ToId == meID ? line1.FromId : line1.ToId; // which end isn't the current point?
                    if (other1 == meID)
                        goto exit; // catastropic error -- both ends are the same and also this point... how would this happen?
                    int other2 = line2.ToId == meID ? line2.FromId : line2.ToId; // which end isn't the current point or other point?
                    if (other2 == meID)
                        goto exit; // catastropic error  -- see above
                    if (other1 == other2)
                        goto exit; // catastropic error -- see above

                    //form a triangle using all three points
                    Vector2 pos1 = new Vector2(positionsData[other1].XPos, positionsData[other1].YPos);
                    Vector2 pos2 = new Vector2(positionsData[other2].XPos, positionsData[other2].YPos);
                    Vector2 center = new Vector2(positionsData[meID].XPos, positionsData[meID].YPos);

                    //check if this is a bend -- has an area
                    if (!IsTriangle(center, pos1, pos2))
                    {
                        processed.Add(meID); // point is spent
                        int next = processed.Contains(other2) ? other1 : other2; // pick the point we haven't visited yet
                        if (lineFrom == -1)
                            lineFrom = next == other2 ? other1 : other2; // from point is the origin point
                        Try(next, tunneled + 1); // tunnel until bend is found to complete the segment
                        return;
                    }
                    //found a bend ... close and complete this tunnel
                    JointPositions.Add(meID);
                    lineTo = meID;
                    return;
                exit:;
                }

                lineFrom = lineTo = -1;
                //begin -- walk the current line until it finds a bend
                //adds what it finds to Lines if it is indeed a line
                //also adds to Joints collection
                Try(positionItem.Key, 0);

                if (lineFrom != -1 && lineTo != -1 && lineFrom != lineTo)
                    Lines.Add(Lines.Count + 1, new GraphLine(lineFrom, lineTo));
            }
        }

        /// <summary>
        /// This function will walk the lineplot created during <see cref="GenerateLineGraph(Dictionary{int, WallGraphPositionEntry}, WallGraphLineEntry[])"/>
        /// finding lines that connect to one another to form shapes making up enclosed spaces.
        /// </summary>
        private void GenerateRoomMap()
        {
            IDictionary<int, IEnumerable<int>> dirtyRooms = new Dictionary<int, IEnumerable<int>>();
            Stack<int> currentlyVisitingRoomPositions = new Stack<int>();

            foreach (var line in Lines)
            {
                int findingPositionID = line.Value.FromPlotID;
                bool FindRoom(int currentLineID, int fromPositionID)
                {
                    currentlyVisitingRoomPositions.Push(currentLineID);
                    var lineValue = Lines[currentLineID];
                    var nextPoint = lineValue.ToPlotID != fromPositionID ? lineValue.ToPlotID : lineValue.FromPlotID;
                    if (nextPoint == findingPositionID)
                    {
                        //found a room
                        int[] points = currentlyVisitingRoomPositions.ToArray();
                        //ensure room is valid
                        //AssertRoomValid(points);
                        //does this room already exist -- also calls AssertRoomValid()
                        if (!RoomExisting(-1, dirtyRooms, points))
                            dirtyRooms.Add(dirtyRooms.Count + 1, points); // doesn't, add it
                        return true;
                    }
                    var results = Lines.Where(x => x.Value.FromPlotID == nextPoint || x.Value.ToPlotID == nextPoint);
                    int nextLineID = -1;
                    foreach (var result in results)
                    {
                        if (result.Key == currentLineID) continue;
                        nextLineID = result.Key;
                    }
                    if (nextLineID == -1) return false;
                    return FindRoom(nextLineID, nextPoint);
                }
                currentlyVisitingRoomPositions.Clear();
                FindRoom(line.Key, findingPositionID);
            }

            Rooms.Clear();
            foreach(var item in dirtyRooms)
                Rooms.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Assures the supplied set of points can make a valid room.
        /// </summary>
        /// <param name="RoomPoints"></param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public void AssertRoomValid(params int[] RoomPoints)
        {
            if (RoomPoints == default)
                throw new NullReferenceException("RoomPoints passed as null reference.");
            if (RoomPoints.Length < 4)
                throw new InvalidDataException($"Rooms must have at least four points. This one supplied has {RoomPoints.Length}");
        }

        /// <summary>
        /// Tests to see if the given room exists in <see cref="Rooms"/>
        /// <para/>Supplying the RoomID will ensure the current room isn't compared to itself in the <see cref="Rooms"/> map.
        /// <para/>It is determined to be the same room if all the points supplied match all the points on a given
        /// Room in the room map.
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="RoomPoints"></param>
        /// <returns></returns>
        public bool RoomExisting(int RoomID, params int[] RoomPoints) => RoomExisting(RoomID, Rooms, RoomPoints);

        /// <summary>
        /// Tests to see if the given room exists in <paramref name="RoomCollection"/>
        /// <para/>Supplying the RoomID will ensure the current room isn't compared to itself in the <paramref name="RoomCollection"/> map.
        /// <para/>It is determined to be the same room if all the points supplied match all the points on a given
        /// Room in the room map.
        /// </summary>
        /// <param name="RoomID"></param>
        /// <param name="RoomPoints"></param>
        /// <returns></returns>
        public bool RoomExisting(int RoomID, IDictionary<int, IEnumerable<int>> RoomCollection, params int[] RoomPoints)
        {
            AssertRoomValid(RoomPoints);
            bool duplicate = false;
            foreach (var otherRoom in RoomCollection)
            {
                if (otherRoom.Key == RoomID)
                    continue; // same as the room I'm comparing
                if (otherRoom.Value.Count() != RoomPoints.Length)
                    continue; // not the same number of points                
                foreach (var point in otherRoom.Value)
                {
                    duplicate = RoomPoints.Contains(point);
                    if (!duplicate) break;
                }
                if (duplicate) break;
            }
            return duplicate;
        }

        public void GenerateImage(string exportFileName, Dictionary<int, WallGraphPositionEntry> positionsData, int Width, int Height)
        {
            Texture2D roomMap = new Texture2D(Width, Height);            

            Color[] colors = new Color[] {
                Color.green,
                Color.cyan,
                Color.magenta,
                Color.grey,
                Color.yellow,
                Color.blue
            };

            foreach (var drawLine in Lines.Values)
            {
                var position = positionsData[drawLine.FromPlotID];
                roomMap.SetPixel((int)position.XPos, (int)position.YPos, Color.red);
                position = positionsData[drawLine.ToPlotID];
                roomMap.SetPixel((int)position.XPos, (int)position.YPos, Color.red);
            }

            int index = -1;
            foreach (var room in Rooms)
            {
                index++;

                Color drawColor = colors[index];
                foreach (var lineID in room.Value)
                {
                    var drawLine = Lines[lineID];
                    var position = positionsData[drawLine.FromPlotID];
                    roomMap.SetPixel((int)position.XPos, (int)position.YPos, drawColor);
                    position = positionsData[drawLine.ToPlotID];
                    roomMap.SetPixel((int)position.XPos, (int)position.YPos, drawColor);
                }
            }

            File.WriteAllBytes(exportFileName, roomMap.EncodeToPNG());
        }
    }
}

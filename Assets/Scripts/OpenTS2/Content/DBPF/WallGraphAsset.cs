using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    public struct WallGraphPositionEntry
    {
        public int Id;
        public float XPos;
        public float YPos;
        public int Level;
        public override string ToString()
        {
            return $"{Id}: ({XPos:x4}, {YPos:x4}, {Level})";
        }
    }

    public struct WallGraphLineEntry
    {
        public int LayerId;
        public int FromId;
        public int Room1;
        public int ToId;
        public int Room2;
        public override string ToString()
        {
            return $"{LayerId} ({FromId}->{ToId}): {Room1} {Room2}";
        }
    }

    public class WallGraphAsset : AbstractAsset
    {
        public int Width { get; }
        public int Height { get; }
        public int Floors { get; private set; }
        public int BaseFloor { get; }
        public Dictionary<int, WallGraphPositionEntry> Positions { get; }
        public int[] Rooms { get; }
        public WallGraphLineEntry[] Lines { get => _lines; set => _lines = value; }
        private WallGraphLineEntry[] _lines;

        public WallGraphAsset(int width, int height, int floors, int baseFloor, WallGraphPositionEntry[] pos, int[] rooms, WallGraphLineEntry[] lines)
        {
            Width = width;
            Height = height;
            Floors = floors;
            BaseFloor = baseFloor;
            Positions = ToPositionDictionary(pos);
            Rooms = rooms;
            Lines = lines;
        }

        private Dictionary<int, WallGraphPositionEntry> ToPositionDictionary(WallGraphPositionEntry[] pos)
        {
            var result = new Dictionary<int, WallGraphPositionEntry>(pos.Length);

            foreach (var position in pos)
            {
                result[position.Id] = position;
            }

            return result;
        }

        bool GetJuncIdByPosition(Vector2 position, int floor, out int juncId, bool createIfNull = true)
        {
            bool existed = false;
            var junctions = Positions.Where(x =>
                x.Value.XPos == position.x && x.Value.YPos == position.y && x.Value.Level == floor);
            juncId = -1;
            if (junctions.Any())
            {
                juncId = junctions.First().Key; // found a matching position
                existed = true;
            }
            else if (createIfNull)
            {
                // make a new position
                if (Positions.Any())
                    juncId = Positions.Select(x => x.Key).Max() + 1;
                else juncId = 0;
                Positions.Add(juncId, new WallGraphPositionEntry()
                {
                    Id = juncId,
                    Level = floor,
                    XPos = position.x,
                    YPos = position.y,
                });
            }

            return existed;
        }

        public bool PushWall(Vector2 pos1, Vector2 pos2, int floor, out int layerID)
        {            
            layerID = -1;

            bool fromExistedAlready = GetJuncIdByPosition(pos1, floor, out int fromId);
            if (fromId == -1) // unable to create !!
                return false;
            bool toExistedAlready = GetJuncIdByPosition(pos2, floor, out int toId);
            if (toId == -1) // unable to create !!
                return false;

            if (fromExistedAlready && toExistedAlready)
            { // wall may already exist
                bool exists = _lines.Any(x => x.ToId == toId && x.FromId == fromId);  // check if this wall segment exists    
                if (exists)
                    return false; // this wall segment already exists.
                exists = _lines.Any(x => x.ToId == fromId && x.FromId == toId);
                if (exists)
                    return false; // this wall segment already exists.
            }

            int layerId = 99;
            if(_lines.Any()) layerId = _lines.Select(x => x.LayerId).Max() + 1;

            Array.Resize(ref _lines, Lines.Length + 1);
            WallGraphLineEntry line = new WallGraphLineEntry()
            {
                FromId = fromId,
                LayerId = layerId,                
                ToId = toId,
            };
            _lines[_lines.Length - 1] = line;
            layerID = layerId;
            if (Floors <= floor)
                Floors++;
            return true;
        }

        private void _delWall(int index)
        {
            int i = index;
            //found the wall
            if (i != _lines.Length - 1) // last wall
                _lines[i] = _lines[_lines.Length - 1]; // move last line to this spot    
            Array.Resize(ref _lines, _lines.Length - 1); // shorten array by one
        }

        internal bool RemoveWall(Vector2 from, Vector2 to, int floor, out int layerID)
        { // should be refactored to batch delete a set of segments instead of one at a time due to iterations being potentially large
            layerID = -1;

            GetJuncIdByPosition(from, floor, out int fromId, false);
            if (fromId == -1) // wall index doesn't exist?
                return false;
            GetJuncIdByPosition(to, floor, out int toId, false);
            if (toId == -1) // wall index doesn't exist?
                return false;

            for (int i = _lines.Length - 1; i >= 0; i--) // reverse order -- since players may more often delete walls they just made
            {
                var line = _lines[i];

                if (toId == fromId) throw new Exception("From and To IDs are the same!");
                if (line.ToId != toId && line.ToId != fromId) continue;
                if (line.FromId != toId && line.FromId != fromId) continue;

                _delWall(i);
                layerID = line.LayerId;
                return true;
            }
            return false; // not found
        }
        /// <summary>
        /// Deletes all the wall lines from the selected floor by their LayerID.
        /// </summary>
        /// <param name="wallLayerIDs"></param>
        /// <returns></returns>
        internal int RemoveWalls(params int[] wallLayerIDs)
        {
            int index = -1;
            int found = 0;
            var deletionIndices = new HashSet<int>();
            foreach(var line in _lines)
            {
                index++;
                if (!wallLayerIDs.Contains(line.LayerId)) continue;
                found++;
                deletionIndices.Add(index);
            }
            int deleted = 0;
            foreach (int i in deletionIndices)
                _delWall(i - deleted++);
            return deleted;
        }
    }
}

using System.Collections.Generic;

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
        public int Floors { get; }
        public int BaseFloor { get; }
        public Dictionary<int, WallGraphPositionEntry> Positions { get; }
        public int[] Rooms { get; }
        public WallGraphLineEntry[] Lines { get; }

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
    }

}
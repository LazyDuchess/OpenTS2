using System;

namespace OpenTS2.Scenes.Lot.Roof
{
    public enum RoofTileBase : byte
    {
        Normal = 0,
        LeftEdge = 1,
        SmallLeftEdge = 2,
        LeftToTop = 3,
        Top = 4,
        TopToRight = 5,
        SmallRightEdge = 6,
        RightEdge = 7,
        
        Empty = 8
    }

    public enum RoofTileIntersection : byte
    {
        None = 0,
        SmallLeft = 1,
        SmallRight = 2,
        Bottom = 3,
        Left = 4,
        Right = 5
    }

    [Flags]
    public enum RoofCut : byte
    {
        None = 0,

        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,

        TopRight = Top | Right,
        TopLeft = Top | Left,
        BottomRight = Bottom | Right,
        BottomLeft = Bottom | Left,

        All = TopLeft | BottomRight
    }

    public struct AtlasIndex
    {
        public int Index;
        public bool Flip;

        public AtlasIndex(int index, bool flip = false)
        {
            Index = index;
            Flip = flip;
        }
    }

    public struct RoofTile
    {
        public static AtlasIndex[][] BAndIToAtlasIndex =
        {
            // Normal = 0,
            new AtlasIndex[]
            {
                new AtlasIndex(9), // None = 0,
                new AtlasIndex(15, true), // SmallLeft = 1,
                new AtlasIndex(15), // SmallRight = 2,
                new AtlasIndex(13), // Bottom = 3,
                new AtlasIndex(2), // Left = 4,
                new AtlasIndex(16), // Right = 5
            },

            // LeftEdge = 1,
            new AtlasIndex[]
            {
                new AtlasIndex(20), // None = 0,
                new AtlasIndex(-1), // SmallLeft = 1,
                new AtlasIndex(-1), // SmallRight = 2,
                new AtlasIndex(12), // Bottom = 3,
                new AtlasIndex(-1), // Left = 4,
                new AtlasIndex(-1), // Right = 5
            },

            // SmallLeftEdge = 2,
            new AtlasIndex[]
            {
                new AtlasIndex(21), // None = 0,
                new AtlasIndex(6), // SmallLeft = 1,
                new AtlasIndex(-1), // SmallRight = 2,
                new AtlasIndex(17), // Bottom = 3,
                new AtlasIndex(-1), // Left = 4,
                new AtlasIndex(10), // Right = 5
            },

            // LeftToTop = 3,
            new AtlasIndex[]
            {
                new AtlasIndex(8), // None = 0,
                new AtlasIndex(7), // SmallLeft = 1,
                new AtlasIndex(-1), // SmallRight = 2,
                new AtlasIndex(3), // Bottom = 3,
                new AtlasIndex(11, true), // Left = 4, // Flip 11 substitute
                new AtlasIndex(11), // Right = 5 // 11 substitute
            },

            // Top = 4,
            new AtlasIndex[]
            {
                new AtlasIndex(22), // None = 0,
                new AtlasIndex(18, true), // SmallLeft = 1,
                new AtlasIndex(18), // SmallRight = 2,
                new AtlasIndex(23), // Bottom = 3,
                new AtlasIndex(11, true), // Left = 4,
                new AtlasIndex(11), // Right = 5
            },

            // TopToRight = 5,
            new AtlasIndex[]
            {
                new AtlasIndex(8, true), // None = 0,
                new AtlasIndex(-1), // SmallLeft = 1,
                new AtlasIndex(7, true), // SmallRight = 2,
                new AtlasIndex(3, true), // Bottom = 3,
                new AtlasIndex(11, true), // Left = 4, // Flip 11 substitute
                new AtlasIndex(11), // Right = 5 // 11 substitute
            },

            // SmallRightEdge = 6,
            new AtlasIndex[]
            {
                new AtlasIndex(14), // None = 0,
                new AtlasIndex(-1), // SmallLeft = 1,
                new AtlasIndex(6, true), // SmallRight = 2,
                new AtlasIndex(17, true), // Bottom = 3,
                new AtlasIndex(10, true), // Left = 4,
                new AtlasIndex(-1), // Right = 5
            },

            // RightEdge = 7,
            new AtlasIndex[]
            {
                new AtlasIndex(1), // None = 0,
                new AtlasIndex(-1), // SmallLeft = 1,
                new AtlasIndex(-1), // SmallRight = 2,
                new AtlasIndex(12, true), // Bottom = 3,
                new AtlasIndex(-1), // Left = 4,
                new AtlasIndex(-1), // Right = 5
            },

            // Empty = 8
            new AtlasIndex[]
            {
                new AtlasIndex(-1), // None = 0,
                new AtlasIndex(-1), // SmallLeft = 1,
                new AtlasIndex(-1), // SmallRight = 2,
                new AtlasIndex(-1), // Bottom = 3,
                new AtlasIndex(-1), // Left = 4,
                new AtlasIndex(-1), // Right = 5
            }
        };

        private RoofTileBase _base;
        private RoofTileIntersection _intersection;
        private RoofCut _cut;

        public RoofTile(RoofTileBase baseTile)
        {
            _base = baseTile;
            _cut = GetBaseCut(baseTile);

            _intersection = RoofTileIntersection.None;
        }

        public void AddIntersection(RoofTileIntersection incoming)
        {
            if (incoming > _intersection && BAndIToAtlasIndex[(int)_base][(int)incoming].Index != -1)
            {
                _intersection = incoming;
            }
        }

        public void Delete()
        {
            _cut = RoofCut.All;
        }

        private static RoofCut RotateCut(RoofCut cut, int dir)
        {
            if (dir == 0)
            {
                return cut;
            }

            return (RoofCut)(((int)cut >> dir) | (((int)cut << (4 - dir)) & (int)RoofCut.All));
        }

        public RoofCut Cut(RoofTile cutWith, int dir)
        {
            _cut |= RoofCut.All ^ RotateCut(cutWith._cut, dir);

            return _cut;
        }

        public RoofCut Cut(RoofCut cutWith, int dir)
        {
            _cut |= RoofCut.All ^ RotateCut(cutWith, dir);

            return _cut;
        }

        public AtlasIndex GetAtlasIndex()
        {
            return BAndIToAtlasIndex[(int)_base][(int)_intersection];
        }

        private static RoofCut GetBaseCut(RoofTileBase baseTile)
        {
            switch (baseTile)
            {
                case RoofTileBase.LeftEdge:
                    return RoofCut.TopLeft;
                case RoofTileBase.RightEdge:
                    return RoofCut.TopRight;
                case RoofTileBase.Empty:
                    return RoofCut.All;
                default:
                    return 0;
            }
        }

        public RoofCut GetBaseCut()
        {
            return GetBaseCut(this._base);
        }

        public RoofCut Cuts()
        {
            return _cut;
        }

        public int CutDir()
        {
            switch (_base)
            {
                case RoofTileBase.LeftEdge:
                    return -1;
                case RoofTileBase.RightEdge:
                    return 1;
                default:
                    return 0;
            }
        }

        public int OverlapPriority()
        {
            if (_cut == RoofCut.All)
            {
                return -1;
            }

            switch (_base)
            {
                case RoofTileBase.Normal:
                    return 4;
                case RoofTileBase.SmallLeftEdge:
                case RoofTileBase.SmallRightEdge:
                    return 3;
                case RoofTileBase.Top:
                    return 2;
                case RoofTileBase.LeftToTop:
                case RoofTileBase.TopToRight:
                    return 1;
                default:
                    return 0;
            }
        }

        public void AddIntersectionsFrom(in RoofTile tile)
        {
            if (tile._intersection != RoofTileIntersection.None)
            {
                AddIntersection(tile._intersection);
            }
        }
    }
}
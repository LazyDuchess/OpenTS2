using OpenTS2.Content.DBPF;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class RoofCollection
    {
        public const uint DefaultGUID = 0x0cdcc049;

        private const int RoofTopId = 0;
        private const int RoofEdgesId = 1;
        private const int RoofTrimId = 2;
        private const int RoofUnderId = 3;

        public uint PatternGUID { get; private set; }

        private List<IRoofType> _roofs;
        private _3DArrayView<float> _elevation;
        private int _baseFloor;
        
        public RoofCollection(RoofEntry[] entries, _3DArrayView<float> elevation, int baseFloor)
        {
            _roofs = new List<IRoofType>();
            _elevation = elevation;
            _baseFloor = baseFloor;

            uint patternGuid = 0;

            foreach (var entry in entries)
            {
                patternGuid = entry.Pattern;
                AddRoof(entry);
            }

            if (patternGuid == 0)
            {
                patternGuid = DefaultGUID;
            }

            PatternGUID = patternGuid;
        }

        private int CalculateElevationIndex(int x, int y)
        {
            return Mathf.Clamp(x, 0, _elevation.Width - 1) * _elevation.Height + Mathf.Clamp(y, 0, _elevation.Height - 1);
        }

        private float InterpHeight(float x, float y, int level)
        {
            float[] data = _elevation.Data[Mathf.Clamp(level - _baseFloor, 0, _elevation.Depth - 1)];

            float i0 = data[CalculateElevationIndex((int)x, (int)y)];
            float i1 = data[CalculateElevationIndex((int)x + 1, (int)y)];
            float j0 = data[CalculateElevationIndex((int)x, (int)y + 1)];
            float j1 = data[CalculateElevationIndex((int)x + 1, (int)y + 1)];

            float xi = x % 1;
            float yi = y % 1;

            return Mathf.Lerp(Mathf.Lerp(i0, i1, xi), Mathf.Lerp(j0, j1, xi), yi);
        }

        public void AddRoof(in RoofEntry entry)
        {
            IRoofType roof;
            float height = InterpHeight(entry.XFrom, entry.YFrom, entry.LevelFrom);

            switch (entry.Type)
            {
                case RoofType.ShedGabled:
                    roof = new ShedGableRoof(entry, height);
                    break;

                case RoofType.ShedHipped:
                    roof = new ShedHipRoof(entry, height);
                    break;

                case RoofType.ShortGable:
                    roof = new GableRoof(entry, height, false);
                    break;

                case RoofType.LongGable:
                    roof = new GableRoof(entry, height, true);
                    break;

                case RoofType.Hip:
                    roof = new HipRoof(entry, height);
                    break;

                case RoofType.Mansard:
                    roof = new MansardRoof(entry, height);
                    break;

                case RoofType.DiagonalLongGable:
                    roof = new DiagonalGableRoof(entry, height, true);
                    break;

                case RoofType.DiagonalShortGable:
                    roof = new DiagonalGableRoof(entry, height, false);
                    break;

                case RoofType.DiagonalHip:
                    roof = new DiagonalHipRoof(entry, height);
                    break;

                case RoofType.DiagonalShedGable:
                    roof = new DiagonalShedGableRoof(entry, height);
                    break;

                case RoofType.DiagonalShedHip:
                    roof = new DiagonalShedHipRoof(entry, height);
                    break;

                case RoofType.PagodaHip:
                    roof = new HipRoof(entry, height, pagoda: true);
                    break;

                case RoofType.PagodaLongGable:
                    roof = new GableRoof(entry, height, true, pagoda: true);
                    break;

                case RoofType.PagodaShedGable:
                    roof = new ShedGableRoof(entry, height, pagoda: true);
                    break;

                case RoofType.DiagonalPagodaHip:
                    roof = new DiagonalHipRoof(entry, height, pagoda: true);
                    break;

                case RoofType.DiagonalPagodaLongGable:
                    roof = new DiagonalGableRoof(entry, height, true, pagoda: true);
                    break;

                case RoofType.DiagonalPagodaShedGable:
                    roof = new DiagonalShedGableRoof(entry, height, pagoda: true);
                    break;

                default:
                    return;
            }

            foreach (var existing in _roofs)
            {
                existing.Intersect(roof);
            }

            _roofs.Add(roof);
        }

        public float GetHeightAt(float x, float y, int floor, float fallback = float.PositiveInfinity, float offset = 0)
        {
            // TODO: fast elimination based on roof rectangle/height?

            float bestHeight = float.NegativeInfinity;

            foreach (var roof in _roofs)
            {
                if (roof.RoofEntry.LevelFrom == floor)
                {
                    float result = roof.GetHeightAt(x, y);

                    if (result != float.PositiveInfinity && result > bestHeight)
                    {
                        bestHeight = result;
                    }
                }
            }

            return bestHeight == float.NegativeInfinity ? fallback : bestHeight - offset;
        }

        public void GenerateGeometry(PatternMeshCollection patterns, int baseFloor)
        {
            int floor = -1;
            RoofGeometryCollection geo = default;

            foreach (var roof in _roofs)
            {
                if (floor != roof.RoofEntry.LevelFrom)
                {
                    floor = roof.RoofEntry.LevelFrom;

                    PatternMeshFloor meshFloor = patterns.GetFloor(floor - baseFloor);
                    geo = new RoofGeometryCollection(
                        meshFloor.Get(RoofTopId),
                        meshFloor.Get(RoofEdgesId),
                        meshFloor.Get(RoofTrimId),
                        meshFloor.Get(RoofUnderId)
                    );
                }

                roof.GenerateGeometry(geo);
            }
        }
    }
}
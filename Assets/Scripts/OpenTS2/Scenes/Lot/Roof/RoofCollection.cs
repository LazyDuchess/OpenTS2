using OpenTS2.Content.DBPF;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class RoofCollection
    {
        private List<IRoofType> _roofs;
        private _3DArrayAsset<float> _elevation;
        
        public RoofCollection(RoofEntry[] entries, _3DArrayAsset<float> elevation)
        {
            _roofs = new List<IRoofType>();
            _elevation = elevation;

            foreach (var entry in entries)
            {
                AddRoof(entry);
            }
        }

        private int CalculateElevationIndex(int x, int y)
        {
            return Mathf.Clamp(x, 0, _elevation.Width - 1) * _elevation.Height + Mathf.Clamp(y, 0, _elevation.Height - 1);
        }

        private float InterpHeight(float x, float y, int level)
        {
            float[] data = _elevation.Data[Mathf.Clamp(level, 0, _elevation.Depth - 1)];

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

                default:
                    return;
            }

            _roofs.Add(roof);
            // TODO: add intersections and tile removals for other roofs.
        }

        public float GetHeightAt(float x, float y)
        {
            // TODO: fast elimination based on roof rectangle/height?

            foreach (var roof in _roofs)
            {
                float result = roof.GetHeightAt(x, y);

                if (result != float.PositiveInfinity)
                {
                    return result;
                }
            }

            return float.PositiveInfinity;
        }

        public void GenerateGeometry(RoofGeometryCollection geo)
        {
            foreach (var roof in _roofs)
            {
                roof.GenerateGeometry(geo);
            }
        }
    }
}
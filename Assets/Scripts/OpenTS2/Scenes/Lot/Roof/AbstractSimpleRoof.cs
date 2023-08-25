using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public abstract class AbstractSimpleRoof : IRoofType
    {
        public RoofEntry RoofEntry => _entry;

        private RoofEntry _entry;

        protected float Height;
        protected RoofEdge[] Edges;

        public AbstractSimpleRoof(RoofEntry entry, float height)
        {
            _entry = entry;
            Height = height;
        }

        public float GetHeightAt(float x, float y)
        {
            foreach (var edge in Edges)
            {
                float result = edge.GetHeightAt(x, y);

                if (result != float.PositiveInfinity)
                {
                    return result;
                }
            }

            return float.PositiveInfinity;
        }

        public void GenerateGeometry(RoofGeometryCollection geo)
        {
            foreach (var edge in Edges)
            {
                edge.GenerateGeometry(geo);
            }
        }

        public static int DetermineDirectionCardinal(in RoofEntry roof)
        {
            var vec = new Vector2(roof.XTo - roof.XFrom, roof.YTo - roof.YFrom);

            if (Math.Abs(vec.y) > Math.Abs(vec.x))
            {
                // Generally pointing in the X direction
                return vec.x > 0 ? 0 : 2;
            }
            else
            {
                // Generally pointing in the Y direction
                return vec.y > 0 ? 1 : 3;
            }
        }

        public void Intersect(IRoofType other)
        {
            if ((other is AbstractSimpleRoof roof))
            {
                foreach (var edge in Edges)
                {
                    foreach (var oedge in roof.Edges)
                    {
                        if (!edge.Intersect(oedge))
                        {
                            edge.RemoveTilesUnder(other);
                            oedge.RemoveTilesUnder(this);
                        }
                    }
                }
            }
            else
            {
                foreach (var edge in Edges)
                {
                    edge.RemoveTilesUnder(other);
                }
            }
        }
    }
}
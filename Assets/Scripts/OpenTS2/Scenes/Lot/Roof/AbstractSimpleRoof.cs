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

        protected static int DetermineDirection(Vector2 vec)
        {
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

        protected static int DetermineDirectionCardinal(in RoofEntry roof)
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

        protected static Vector2 GetDiagonalVector(in RoofEntry roof)
        {
            var vec = new Vector2(roof.XTo - roof.XFrom, roof.YTo - roof.YFrom);

            // Vector is composed of two components: (1, 1)*xD + (-1, 1)*yD = vec
            // vec.x = xD - yD
            // vec.y = xD + yD
            // Try and find those components.

            // vec.x = (vec.y - yD) - yD
            // vec.x = vec.y - 2yD
            // yD = (vec.y - vec.x) / 2

            float yD = (vec.y - vec.x) / 2;
            float xD = vec.y - yD;

            return new Vector2(xD, yD);
        }

        protected static Vector2 VectorFromDiagonal(Vector2 diag)
        {
            return new Vector2(diag.x - diag.y, diag.x + diag.y);
        }

        protected static Vector2 RotateVector(Vector2 vec, int dir)
        {
            switch (dir)
            {
                case 1:
                    return new Vector2(-vec.y, vec.x);
                case 2:
                    return new Vector2(-vec.x, -vec.y);
                case 3:
                    return new Vector2(vec.y, -vec.x);
            }

            return vec;
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
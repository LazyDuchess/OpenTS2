using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class DiagonalGableRoof : AbstractSimpleRoof
    {
        public DiagonalGableRoof(RoofEntry entry, float height, bool isLong) : base(entry, height)
        {
            Vector2 diagVec = GetDiagonalVector(entry);
            int dir = DetermineDirection(diagVec);

            Vector2 from = new Vector2(entry.XFrom, entry.YFrom);

            float minX = Math.Min(0, diagVec.x);
            float maxX = Math.Max(0, diagVec.x);
            float minY = Math.Min(0, diagVec.y);
            float maxY = Math.Max(0, diagVec.y);

            Vector2 bl = from + VectorFromDiagonal(new Vector2(minX, minY));
            Vector2 tr = from + VectorFromDiagonal(new Vector2(maxX, maxY));
            Vector2 br = from + VectorFromDiagonal(new Vector2(maxX, minY));
            Vector2 tl = from + VectorFromDiagonal(new Vector2(minX, maxY));

            bool flatInX = (dir % 2) == (isLong ? 1 : 0);
            float slope = entry.RoofAngle;

            if (flatInX)
            {
                Vector2 toTop = VectorFromDiagonal(new Vector2(0, (maxY - minY) / 2));

                Edges = new RoofEdge[]
                {
                    new RoofEdge(height, slope, bl, br, br + toTop, bl + toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    new RoofEdge(height, slope, tr, tl, tl - toTop, tr - toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat)
                };
            }
            else
            {
                Vector2 toTop = VectorFromDiagonal(new Vector2((maxX - minX) / 2, 0));

                Edges = new RoofEdge[]
                {
                    new RoofEdge(height, slope, br, tr, tr - toTop, br - toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    new RoofEdge(height, slope, tl, bl, bl + toTop, tl + toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat)
                };
            }
        }
    }
}
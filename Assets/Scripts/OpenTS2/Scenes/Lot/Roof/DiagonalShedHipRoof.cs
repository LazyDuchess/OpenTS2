using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class DiagonalShedHipRoof : AbstractSimpleRoof
    {
        public DiagonalShedHipRoof(RoofEntry entry, float height) : base(entry, height)
        {
            Vector2 diagVec = GetDiagonalVector(entry);
            int dir = DetermineDirection(diagVec);

            Vector2 from = new Vector2(entry.XFrom, entry.YFrom);

            float lowX = Math.Min(0, diagVec.x);
            float hiX = Math.Max(0, diagVec.x);
            float lowY = Math.Min(0, diagVec.y);
            float hiY = Math.Max(0, diagVec.y);

            Vector2 bl = from + VectorFromDiagonal(new Vector2(lowX, lowY));
            Vector2 tr = from + VectorFromDiagonal(new Vector2(hiX, hiY));
            Vector2 br = from + VectorFromDiagonal(new Vector2(hiX, lowY));
            Vector2 tl = from + VectorFromDiagonal(new Vector2(lowX, hiY));

            Vector2[] corners = new Vector2[]
            {
                tl, bl, br, tr
            };

            bool flatInX = ((dir % 2) == 0);

            float minDim = flatInX ?
                Math.Min(hiX - lowX, (hiY - lowY) / 2) : // In x direction
                Math.Min((hiX - lowX) / 2, hiY - lowY);  // In y direction

            Vector2 towardsDirection = VectorFromDiagonal(RotateVector(new Vector2(minDim, 0), dir));
            Vector2 towardsTop = VectorFromDiagonal(RotateVector(new Vector2(0, minDim), dir));

            Vector2 c1 = corners[dir];
            Vector2 c2 = corners[(dir + 1) % 4];
            Vector2 c3 = corners[(dir + 2) % 4];
            Vector2 c4 = corners[(dir + 3) % 4];

            // See gable roof for notes on this.
            Vector2 overhang = VectorFromDiagonal(RotateVector(new Vector2(0.5f, 0), dir));
            towardsDirection += overhang;

            float slope = entry.RoofAngle;

            Edges = new RoofEdge[]
            {
                new RoofEdge(height, slope, c4, c1, c1 - towardsTop, c4 - towardsTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Normal),
                new RoofEdge(height, slope, c1, c2, c2 + towardsDirection, c1 + towardsDirection, topOverhangSize: 2),
                new RoofEdge(height, slope, c2, c3, c3 + towardsTop, c2 + towardsTop, RoofEdgeEnd.Normal, RoofEdgeEnd.Flat),
            };
        }
    }
}
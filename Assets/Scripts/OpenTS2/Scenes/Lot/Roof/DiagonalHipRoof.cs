using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class DiagonalHipRoof : AbstractSimpleRoof
    {
        public DiagonalHipRoof(RoofEntry entry, float height) : base(entry, height)
        {
            Vector2 size = GetDiagonalVector(entry);
            Vector2 from = new Vector2(entry.XFrom, entry.YFrom);

            float lowX = Math.Min(0, size.x);
            float hiX = Math.Max(0, size.x);
            float lowY = Math.Min(0, size.y);
            float hiY = Math.Max(0, size.y);

            Vector2 bl = from + VectorFromDiagonal(new Vector2(lowX, lowY));
            Vector2 tr = from + VectorFromDiagonal(new Vector2(hiX, hiY));
            Vector2 br = from + VectorFromDiagonal(new Vector2(hiX, lowY));
            Vector2 tl = from + VectorFromDiagonal(new Vector2(lowX, hiY));

            float minDim = Math.Min(Math.Abs(size.x), Math.Abs(size.y)) / 2;

            Vector2 minX = VectorFromDiagonal(new Vector2(minDim, 0));
            Vector2 minY = VectorFromDiagonal(new Vector2(0, minDim));

            float slope = entry.RoofAngle;

            Edges = new RoofEdge[]
            {
                new RoofEdge(height, slope, bl, br, br + minY, bl + minY), // Bottom
                new RoofEdge(height, slope, br, tr, tr - minX, br - minX), // Right
                new RoofEdge(height, slope, tr, tl, tl - minY, tr - minY), // Top
                new RoofEdge(height, slope, tl, bl, bl + minX, tl + minX), // Left
            };
        }
    }
}
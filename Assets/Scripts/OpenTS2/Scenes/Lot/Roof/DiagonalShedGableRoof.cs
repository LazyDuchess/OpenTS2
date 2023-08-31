using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class DiagonalShedGableRoof : AbstractSimpleRoof
    {
        public DiagonalShedGableRoof(RoofEntry entry, float height) : base(entry, height)
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

            // "Towards" means that the wall edge is facing that direction.

            // For some reason, diagonal normal shed roofs have a top overhang. None of the other shed roofs have this.
            // The overhang has some weird properties. It has overhang rules for cutting out roofs below it,
            // but it does alter wall height, which suggests it's physically present unlike other overhangs.
            Vector2 overhang = VectorFromDiagonal(RotateVector(new Vector2(0.5f, 0), dir));

            float slope = entry.RoofAngle;

            switch (dir)
            {
                case 0: // Towards positive x
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, tl, bl, br + overhang, tr + overhang, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, 2),
                    };
                    break;
                case 1: // Towards positive y
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, bl, br, tr + overhang, tl + overhang, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, 2),
                    };
                    break;
                case 2: // Towards negative x
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, br, tr, tl + overhang, bl + overhang, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, 2),
                    };
                    break;
                case 3: // Towards negative y
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, tr, tl, bl + overhang, br + overhang, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, 2),
                    };
                    break;
            }
        }
    }
}
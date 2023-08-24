using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class ShedHipRoof : AbstractSimpleRoof
    {
        private Vector2 RotateVector(Vector2 vec, int dir)
        {
            switch (dir)
            {
                case 1:
                    return new Vector2(-vec.y, vec.x);
                case 2:
                    return new Vector2(-vec.x, -vec.y);
                case 3:
                    return new Vector2(vec.x, -vec.y);
            }

            return vec;
        }

        public ShedHipRoof(RoofEntry entry, float height) : base(entry, height)
        {
            int dir = DetermineDirectionCardinal(entry);

            Vector2 bl = new Vector2(Math.Min(entry.XFrom, entry.XTo), Math.Min(entry.YFrom, entry.YTo));
            Vector2 tr = new Vector2(Math.Max(entry.XFrom, entry.XTo), Math.Max(entry.YFrom, entry.YTo));
            Vector2 br = new Vector2(tr.x, bl.y);
            Vector2 tl = new Vector2(bl.x, tr.y);

            Vector2[] corners = new Vector2[]
            {
                tl, bl, br, tr
            };

            bool flatInX = ((dir % 2) == 0);

            float minDim = flatInX ?
                Math.Min(tr.x - bl.x, (tr.y - bl.y) / 2) : // In x direction
                Math.Min((tr.x - bl.x) / 2, tr.y - bl.y);  // In y direction

            Vector2 towardsDirection = RotateVector(new Vector2(minDim, 0), dir);
            Vector2 towardsTop = RotateVector(new Vector2(0, minDim), dir);

            Vector2 c1 = corners[dir];
            Vector2 c2 = corners[(dir + 1) % 4];

            float slope = entry.RoofAngle;

            Edges = new RoofEdge[]
            {
                new RoofEdge(height, slope, corners[(dir + 3) % 4], c1, c2 - towardsTop, c1 - towardsTop, true, false),
                new RoofEdge(height, slope, c1, c2, c2 + towardsDirection, c1 + towardsDirection),
                new RoofEdge(height, slope, c2, corners[(dir + 2) % 4], c2 + towardsTop, c1 + towardsTop, false, true),
            };
        }
    }
}
using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class GableRoof : AbstractSimpleRoof
    {
        public GableRoof(RoofEntry entry, float height, bool isLong, bool pagoda = false) : base(entry, height)
        {
            int dir = DetermineDirectionCardinal(entry);

            Vector2 bl = new Vector2(Math.Min(entry.XFrom, entry.XTo), Math.Min(entry.YFrom, entry.YTo));
            Vector2 tr = new Vector2(Math.Max(entry.XFrom, entry.XTo), Math.Max(entry.YFrom, entry.YTo));
            Vector2 br = new Vector2(tr.x, bl.y);
            Vector2 tl = new Vector2(bl.x, tr.y);

            bool flatInX = (dir % 2) == (isLong ? 1 : 0);
            float slope = entry.RoofAngle;

            if (flatInX)
            {
                Vector2 toTop = new Vector2(0, (tr.y - br.y) / 2);

                Edges = new RoofEdge[]
                {
                    new RoofEdge(height, slope, bl, br, br + toTop, bl + toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, pagoda: pagoda),
                    new RoofEdge(height, slope, tr, tl, tl - toTop, tr - toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, pagoda: pagoda)
                };
            }
            else
            {
                Vector2 toTop = new Vector2((tr.x - tl.x) / 2, 0);

                Edges = new RoofEdge[]
                {
                    new RoofEdge(height, slope, br, tr, tr - toTop, br - toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, pagoda: pagoda),
                    new RoofEdge(height, slope, tl, bl, bl + toTop, tl + toTop, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat, pagoda: pagoda)
                };
            }
        }
    }
}
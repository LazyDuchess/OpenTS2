using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class ShedGableRoof : AbstractSimpleRoof
    {
        public ShedGableRoof(RoofEntry entry, float height) : base(entry, height)
        {
            int dir = DetermineDirectionCardinal(entry);

            Vector2 bl = new Vector2(Math.Min(entry.XFrom, entry.XTo), Math.Min(entry.YFrom, entry.YTo));
            Vector2 tr = new Vector2(Math.Max(entry.XFrom, entry.XTo), Math.Max(entry.YFrom, entry.YTo));
            Vector2 br = new Vector2(tr.x, bl.y);
            Vector2 tl = new Vector2(bl.x, tr.y);

            // "Towards" means that the wall edge is facing that direction.

            float slope = entry.RoofAngle;

            switch (dir)
            {
                case 0: // Towards positive x
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, tl, bl, br, tr, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    };
                    break;
                case 1: // Towards positive y
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, bl, br, tr, tl, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    };
                    break;
                case 2: // Towards negative x
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, br, tr, tl, bl, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    };
                    break;
                case 3: // Towards negative y
                    Edges = new RoofEdge[]
                    {
                        new RoofEdge(height, slope, tr, tl, bl, br, RoofEdgeEnd.Flat, RoofEdgeEnd.Flat),
                    };
                    break;
            }
        }
    }
}
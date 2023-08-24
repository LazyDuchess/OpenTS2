using OpenTS2.Content.DBPF;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public class HipRoof : AbstractSimpleRoof
    {
        public HipRoof(RoofEntry entry, float height) : base(entry, height) 
        {
            Vector2 bl = new Vector2(Math.Min(entry.XFrom, entry.XTo), Math.Min(entry.YFrom, entry.YTo));
            Vector2 tr = new Vector2(Math.Max(entry.XFrom, entry.XTo), Math.Max(entry.YFrom, entry.YTo));

            Vector2 size = tr - bl;

            float minDim = Math.Min(size.x, size.y) / 2;

            Vector2 minX = new Vector2(minDim, 0);
            Vector2 minY = new Vector2(0, minDim);

            Vector2 br = new Vector2(tr.x, bl.y);
            Vector2 tl = new Vector2(bl.x, tr.y);

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
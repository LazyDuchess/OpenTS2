namespace OpenTS2.Scenes.Lot.Roof
{
    public readonly struct RoofGeometryCollection
    {
        public readonly PatternMesh RoofTop;
        public readonly PatternMesh RoofEdges;
        public readonly PatternMesh RoofTrim;
        public readonly PatternMesh RoofUnder;

        public RoofGeometryCollection(
            PatternMesh roofTop,
            PatternMesh roofEdges,
            PatternMesh roofTrim,
            PatternMesh roofUnder)
        {
            RoofTop = roofTop;
            RoofEdges = roofEdges;
            RoofTrim = roofTrim;
            RoofUnder = roofUnder;
        }

        public bool Valid()
        {
            return RoofTop != null;
        }

        public void Clear()
        {
            RoofTop.Component?.Clear();
            RoofEdges.Component?.Clear();
            RoofTrim.Component?.Clear();
            RoofUnder.Component?.Clear();
        }

        public void Commit()
        {
            RoofTop.Component?.Commit();
            RoofEdges.Component?.Commit();
            RoofTrim.Component?.Commit();
            RoofUnder.Component?.Commit();
        }
    }
}
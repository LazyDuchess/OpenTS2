using OpenTS2.Content.DBPF;

namespace OpenTS2.Scenes.Lot.Roof
{
    public interface IRoofType
    {
        public RoofEntry RoofEntry { get; }

        float GetHeightAt(float x, float y);

        void GenerateGeometry(RoofGeometryCollection geo);

        public void Intersect(IRoofType other);
    }
}
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class PatternMesh
    {
        public GameObject Object;
        public LotFloorPatternComponent Component;

        public PatternMesh(GameObject parent, string name, Material material)
        {
            Object = new GameObject(name, typeof(LotFloorPatternComponent));
            Component = Object.GetComponent<LotFloorPatternComponent>();

            Object.transform.SetParent(parent.transform);
            Component.Initialize(material);
        }
    }
}
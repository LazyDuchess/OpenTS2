using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class PatternMesh
    {
        public GameObject Object;
        public LotArchitectureMeshComponent Component;

        public PatternMesh(GameObject parent, string name, Material material)
        {
            Object = new GameObject(name, typeof(LotArchitectureMeshComponent));
            Component = Object.GetComponent<LotArchitectureMeshComponent>();

            Object.transform.SetParent(parent.transform);
            Component.Initialize(material);
        }
    }
}
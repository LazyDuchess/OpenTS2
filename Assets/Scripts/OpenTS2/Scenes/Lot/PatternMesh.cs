using OpenTS2.Scenes.Lot.State;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class PatternMesh
    {
        public GameObject Object;
        public LotArchitectureMeshComponent Component;

        private string _name;
        private Material _material;
        private Dictionary<Texture2D, PatternMesh> _maskMeshes;

        public PatternMesh(GameObject parent, string name, Material material, bool extraUV = false)
        {
            _name = name;
            _material = material;

            Object = new GameObject(name, typeof(LotArchitectureMeshComponent));
            Component = Object.GetComponent<LotArchitectureMeshComponent>();

            if (extraUV)
            {
                Component.EnableExtraUV();
            }

            Object.transform.SetParent(parent.transform);
            Component.Initialize(material);
        }

        public PatternMesh GetForMask(Texture2D mask)
        {
            _maskMeshes ??= new Dictionary<Texture2D, PatternMesh>();

            if (!_maskMeshes.TryGetValue(mask, out PatternMesh mesh))
            {
                Material material = new Material(_material);

                // TODO: Bind the mask as a mask texture.

                mesh = new PatternMesh(Object, $"{_name}_mask_{mask.GetHashCode():x8}", material);
            }

            return mesh;
        }

        public void Clear()
        {
            Component.Clear();

            if (_maskMeshes != null)
            {
                foreach (PatternMesh pattern in _maskMeshes.Values)
                {
                    pattern.Clear();
                }
            }
        }

        public bool Commit()
        {
            // TODO: Eventually, committing an empty mesh should delete the game object for that pattern/component.

            bool hasData = false;

            hasData |= Component.Commit();

            if (_maskMeshes != null)
            {
                foreach (PatternMesh pattern in _maskMeshes.Values)
                {
                    hasData |= pattern.Commit();
                }
            }

            return hasData;
        }

        public void UpdateDisplay(WorldState state, bool visible, bool isTop)
        {
            // TODO: Walls cutaway
            Component.SetVisible(visible);
        }
    }
}
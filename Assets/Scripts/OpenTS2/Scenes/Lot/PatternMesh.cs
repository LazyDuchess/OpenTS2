using OpenTS2.Scenes.Lot.State;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public interface IPatternMaterialConfigurator
    {
        public bool ExtraUV { get; }
        public void Configure(WorldState state, bool visible, bool isTop, Material mat);
        public void AlterMeshBounds(Mesh mesh);
    }

    public class PatternMesh
    {
        public GameObject Object;
        public LotArchitectureMeshComponent Component;

        private string _name;
        private Material _material;
        private Dictionary<Texture2D, PatternMesh> _maskMeshes;
        private IPatternMaterialConfigurator _matConfig;

        public PatternMesh(GameObject parent, string name, Material material, IPatternMaterialConfigurator matConfig = null)
        {
            _name = name;
            _material = new Material(material);
            _matConfig = matConfig;

            Object = new GameObject(name, typeof(LotArchitectureMeshComponent));
            Component = Object.GetComponent<LotArchitectureMeshComponent>();

            if (matConfig?.ExtraUV == true)
            {
                Component.EnableExtraUV();
            }

            Object.transform.SetParent(parent.transform);
            Component.Initialize(_material);
        }

        public PatternMesh GetForMask(Texture2D mask)
        {
            _maskMeshes ??= new Dictionary<Texture2D, PatternMesh>();

            if (!_maskMeshes.TryGetValue(mask, out PatternMesh mesh))
            {
                Material material = new Material(_material);

                material.SetTexture("_TexMask", mask);

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

            if (hasData)
            {
                _matConfig?.AlterMeshBounds(Component.Mesh);
            }

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
            _matConfig?.Configure(state, visible, isTop, _material);

            Component.SetVisible(visible);

            if (_maskMeshes != null)
            {
                foreach (PatternMesh pattern in _maskMeshes.Values)
                {
                    pattern.UpdateDisplay(state, visible, isTop);
                }
            }
        }
    }
}
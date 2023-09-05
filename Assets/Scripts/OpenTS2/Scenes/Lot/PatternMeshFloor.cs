using OpenTS2.Content;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public readonly struct PatternDescriptor
    {
        public readonly string Name;
        public readonly Material Material;

        public PatternDescriptor(string name, Material material)
        {
            Name = name;
            Material = material;
        }
    }

    public readonly struct PatternVariant
    {
        public readonly string Name;
        public readonly Func<Material, Material> MaterialTransform;
        public readonly bool ExtraUV;

        public PatternVariant(string name, Func<Material, Material> materialTransform = null, bool extraUV = false)
        {
            Name = name;
            MaterialTransform = materialTransform;
            ExtraUV = extraUV;
        }
    }

    public class PatternMeshFloor
    {
        public GameObject Object;

        private PatternVariant[] _variants;

        private PatternDescriptor[] _patterns;

        private PatternMesh[] _patternMeshes;

        private Dictionary<uint, FenceCollection> _fenceByGuid;

        public PatternMeshFloor(GameObject parent, int id, PatternVariant[] variants, PatternDescriptor[] patterns)
        {
            Object = new GameObject($"Floor {id}");
            Object.transform.SetParent(parent.transform);

            _variants = variants;

            UpdatePatterns(patterns);
        }

        public void UpdatePatterns(PatternDescriptor[] patterns)
        {
            _patterns = patterns;

            _patternMeshes = new PatternMesh[patterns.Length * (1 + _variants.Length)];
        }

        public PatternMesh Get(int id, int variantId = 0)
        {
            int key = id + variantId * _patterns.Length;

            PatternMesh result = _patternMeshes[key];

            if (result == null)
            {
                // Make a new one.

                ref var descriptor = ref _patterns[id];

                if (descriptor.Material == null)
                {
                    // Never mind.
                    return null;
                }

                if (variantId > 0)
                {
                    ref var variant = ref _variants[variantId - 1];

                    result = new PatternMesh(
                        Object,
                        $"{descriptor.Name}_{variant.Name}",
                        variant.MaterialTransform != null ? variant.MaterialTransform(descriptor.Material) : descriptor.Material,
                        variant.ExtraUV);
                }
                else
                {
                    result = new PatternMesh(Object, descriptor.Name, descriptor.Material);
                }

                _patternMeshes[key] = result;
            }

            return result;
        }

        public FenceCollection GetFence(uint guid)
        {
            _fenceByGuid ??= new Dictionary<uint, FenceCollection>();

            if (!_fenceByGuid.TryGetValue(guid, out FenceCollection result))
            {
                result = new FenceCollection(ContentProvider.Get(), Object, guid);

                _fenceByGuid[guid] = result;
            }

            return result;
        }

        public void Clear()
        {
            foreach (PatternMesh pattern in _patternMeshes)
            {
                pattern?.Clear();
            }

            if (_fenceByGuid != null)
            {
                foreach (FenceCollection fences in _fenceByGuid.Values)
                {
                    fences.Clear();
                }
            }
        }

        public bool Commit()
        {
            bool hasData = false;

            foreach (PatternMesh pattern in _patternMeshes)
            {
                hasData |= pattern?.Commit() ?? false;
            }

            return hasData;
        }

        public void UpdateDisplay(WorldState state, bool visible, bool isTop)
        {
            foreach (PatternMesh pattern in _patternMeshes)
            {
                pattern?.UpdateDisplay(state, visible, isTop);
            }

            if (_fenceByGuid != null)
            {
                foreach (FenceCollection fences in _fenceByGuid.Values)
                {
                    fences.SetVisible(visible);
                }
            }
        }
    }
}
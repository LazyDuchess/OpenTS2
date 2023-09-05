using OpenTS2.Scenes.Lot.State;
using System;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public enum DisplayUpdateType
    {
        Default,
        Roof,
        Wall
    }

    public struct PatternMeshCollection
    {
        private GameObject _parent;
        private PatternDescriptor[] _patterns;
        private PatternVariant[] _variants;

        private PatternMeshFloor[] _floors;

        public PatternMeshCollection(GameObject parent, PatternDescriptor[] patterns, PatternVariant[] variants, int floorCount)
        {
            _parent = parent;
            _patterns = patterns;
            _variants = variants;

            _floors = new PatternMeshFloor[floorCount];
        }

        public void SetFloorCount(int floorCount)
        {
            if (_floors.Length != floorCount)
            {
                Array.Resize(ref _floors, floorCount);
            }
        }

        public PatternMeshFloor GetFloor(int floor)
        {
            PatternMeshFloor result = _floors[floor];

            if (result == null)
            {
                result = new PatternMeshFloor(_parent, floor, _variants, _patterns);

                _floors[floor] = result;
            }

            return result;
        }

        public void UpdatePatterns(PatternDescriptor[] patterns)
        {
            foreach (var floor in _floors)
            {
                floor?.UpdatePatterns(patterns);
            }
        }

        public void ClearAll()
        {
            foreach (PatternMeshFloor floor in _floors)
            {
                floor?.Clear();
            }
        }

        public bool CommitAll()
        {
            bool hasData = false;

            foreach (PatternMeshFloor floor in _floors)
            {
                hasData |= floor?.Commit() ?? false;
            }

            return hasData;
        }

        public void UpdateDisplay(WorldState state, int baseLevel, DisplayUpdateType type = DisplayUpdateType.Default)
        {
            int topLevel = state.Level - baseLevel;

            if (type == DisplayUpdateType.Roof)
            {
                if (state.Walls != WallsMode.Roof)
                {
                    topLevel = -1;
                }
            }
            else if (type == DisplayUpdateType.Wall && state.Walls < WallsMode.Up)
            {
                // Temporary - the vertex shader should be doing this.
                topLevel--;
            }

            for (int i = 0; i < _floors.Length; i++)
            {
                bool visible = i < topLevel;
                bool isTop = i == topLevel - 1;

                _floors[i]?.UpdateDisplay(state, visible, isTop);
            }
        }
    }

}
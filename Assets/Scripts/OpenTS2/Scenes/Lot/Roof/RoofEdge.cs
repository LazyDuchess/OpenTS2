using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    /// <summary>
    /// A roof edge describes one side of a roof.
    /// This keeps track of an "intersection line" which represents the
    /// highest intersecting line of any roof across the surface.
    /// </summary>
    public class RoofEdge
    {
        private const float ThicknessTop = 0.075f;
        private const float ThicknessInner = 0.15f;
        private const float ThicknessTotal = 0.075f;
        private const float Overflow = 0.5f;

        private const float Bias = 0.0001f;
        private List<Vector2> _intersectionLine;

        private float _height;
        private float _slope;

        private Vector2 _bl;
        private Vector2 _br;
        private Vector2 _tr;
        private Vector2 _tl;

        private Vector2 _yNormal;
        private Vector2 _xNormal;

        private float _bPc;
        private float _rPc;
        private float _tPc;
        private float _lPc;

        private bool _lFlat;
        private bool _rFlat;

        private Vector3[] _vertices;
        private Vector2[] _uvs;

        public RoofEdge(float height, float slope, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, bool lFlat = false, bool rFlat = false)
        {
            _intersectionLine = new List<Vector2>();

            _height = height;
            _slope = slope;

            _yNormal = tl - bl;
            _xNormal = br - bl;

            _yNormal.Normalize();
            _xNormal.Normalize();

            _bPc = Vector2.Dot(_yNormal, bl);
            _rPc = Vector2.Dot(_xNormal, br);
            _tPc = Vector2.Dot(_yNormal, tr);
            _lPc = Vector2.Dot(_xNormal, tl);

            _lFlat = lFlat;
            _rFlat = rFlat;

            _vertices = new Vector3[4];
            _uvs = new Vector2[4];
        }

        public void GenerateGeometry(RoofGeometryCollection geo)
        {
            var topComp = geo.RoofTop.Component;
        }

        public float GetHeightAt(float x, float y)
        {
            Vector2 pos = new Vector2(x, y);

            float dotX = Vector2.Dot(pos, _xNormal);
            float dotY = Vector2.Dot(pos, _yNormal);

            if (dotX < _lPc || dotX > _rPc || dotY < _bPc || dotY > _tPc)
            {
                return float.PositiveInfinity;
            }

            float yDist = dotY - _bPc;

            if ((!_lFlat && dotX - _lPc < yDist) || (!_rFlat && _rPc - dotX < yDist))
            {
                // If the edges are not flat, also cut off top 45 degrees from left and right.
                return float.PositiveInfinity;
            }

            return yDist * (_slope * 2) + _height;
        }
    }
}
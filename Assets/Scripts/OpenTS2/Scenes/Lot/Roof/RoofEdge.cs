using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        private const float ThicknessTotal = ThicknessTop + ThicknessInner;
        private const float TileSize = 0.5f;

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

        // Roof Tiles
        private RoofTile[] _tiles;
        private int _tileWidth;
        private int _tileHeight;

        private float _tileScale;

        private Vector3[] _vertices;
        private Vector3[] _edgeVertices;
        private Vector2[] _uvs;
        private Vector2[] _edgeUVs;

        public RoofEdge(float height, float slope, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, bool lFlat = false, bool rFlat = false)
        {
            _intersectionLine = new List<Vector2>();

            _height = height;
            _slope = slope;

            _bl = bl;
            _tl = tl; // Used
            _br = br;
            _tr = tr;

            _yNormal = tl - bl;
            _xNormal = br - bl;

            float xDist = _xNormal.magnitude;
            float yDist = _yNormal.magnitude;

            _yNormal.Normalize();
            _xNormal.Normalize();

            _bPc = Vector2.Dot(_yNormal, bl);
            _rPc = Vector2.Dot(_xNormal, br);
            _tPc = Vector2.Dot(_yNormal, tr);
            _lPc = Vector2.Dot(_xNormal, tl);

            _lFlat = lFlat;
            _rFlat = rFlat;

            _vertices = new Vector3[4];
            _edgeVertices = new Vector3[4];
            _uvs = new Vector2[4];
            _edgeUVs = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
            };

            _tileWidth = Mathf.RoundToInt(xDist / TileSize) + 2;
            _tileHeight = Mathf.RoundToInt(yDist / TileSize) + 1;
            _tiles = new RoofTile[_tileWidth * _tileHeight];

            InitTiles();
        }

        private void InitTiles()
        {
            int mid = 0;

            if (!_rFlat && !_lFlat)
            {
                mid = _tileWidth / 2;
            }
            else if (!_lFlat)
            {
                mid = _tileWidth;
            }

            int i = 0;
            for (int y = 0; y < _tileHeight; y++)
            {
                for (int x = 0; x < _tileWidth; x++, i++)
                {
                    ref RoofTile tile = ref _tiles[i];

                    int side = x + y;
                    int oside = ((_tileWidth - 1) - x) + y;

                    if (!_lFlat && x < mid)
                    {
                        if (side <= _tileHeight)
                        {
                            if (side == _tileHeight)
                            {
                                tile = new RoofTile(y != 0 ? RoofTileBase.SmallLeftEdge : RoofTileBase.LeftToTop);
                            }
                            else if (side == _tileHeight - 1)
                            {
                                tile = new RoofTile(RoofTileBase.LeftEdge);
                            }
                            else
                            {
                                tile.Delete();
                            }

                            continue;
                        }
                    }

                    if (!_rFlat && x >= mid)
                    {
                        if (oside <= _tileHeight)
                        {
                            if (oside == _tileHeight)
                            {
                                tile = new RoofTile(y != 0 ? RoofTileBase.SmallRightEdge : RoofTileBase.TopToRight);
                            }
                            else if (oside == _tileHeight - 1)
                            {
                                tile = new RoofTile(RoofTileBase.RightEdge);
                            }
                            else
                            {
                                tile.Delete();
                            }

                            continue;
                        }
                    }

                    tile = new RoofTile(y == 0 ? RoofTileBase.Top : RoofTileBase.Normal);
                }
            }
        }

        private void DrawTrim(LotFloorPatternComponent trimComp, LotFloorPatternComponent edgeComp, Vector3 from, Vector3 to)
        {
            _edgeVertices[0] = from;
            _edgeVertices[1] = to;
            _edgeVertices[2] = to + new Vector3(0, -ThicknessTop, 0);
            _edgeVertices[3] = from + new Vector3(0, -ThicknessTop, 0);

            int trimVertex = trimComp.GetVertexIndex();

            trimComp.AddVertices(_edgeVertices, _edgeUVs);
            trimComp.AddTriangle(trimVertex, 0, 1, 2);
            trimComp.AddTriangle(trimVertex, 2, 3, 0);

            _edgeVertices[0] = _edgeVertices[3];
            _edgeVertices[1] = _edgeVertices[2];
            _edgeVertices[2] = _edgeVertices[2] + new Vector3(0, -ThicknessInner, 0);
            _edgeVertices[3] = _edgeVertices[3] + new Vector3(0, -ThicknessInner, 0);

            int edgeVertex = edgeComp.GetVertexIndex();

            edgeComp.AddVertices(_edgeVertices, _edgeUVs);
            edgeComp.AddTriangle(edgeVertex, 0, 1, 2);
            edgeComp.AddTriangle(edgeVertex, 2, 3, 0);
        }

        private int AddUndersideVerts(LotFloorPatternComponent underComp)
        {
            var off = new Vector3(0, -ThicknessTotal, 0);
            int index = underComp.GetVertexIndex();

            _edgeVertices[0] = _vertices[0] + off;
            _edgeVertices[1] = _vertices[1] + off;
            _edgeVertices[2] = _vertices[2] + off;
            _edgeVertices[3] = _vertices[3] + off;

            underComp.AddVertices(_edgeVertices, _edgeUVs);

            return index;
        }

        public void GenerateGeometry(RoofGeometryCollection geo)
        {
            var trimComp = geo.RoofTrim.Component;
            var edgeComp = geo.RoofEdges.Component;
            var topComp = geo.RoofTop.Component;
            var underComp = geo.RoofUnder.Component;

            Vector3 basePos = new Vector3(_tl.x, _height + (_tileHeight - 1) * _slope + ThicknessTop, _tl.y);

            Vector3 xTile = new Vector3(_xNormal.x * TileSize, 0, _xNormal.y * TileSize);
            Vector3 yTile = new Vector3(_yNormal.x * -TileSize, -_slope, _yNormal.y * -TileSize);

            basePos -= xTile;

            int i = 0;
            for (int y = 0; y < _tileHeight; y++)
            {
                for (int x = 0; x < _tileWidth; x++, i++)
                {
                    ref RoofTile tile = ref _tiles[i];

                    AtlasIndex graphic = tile.GetAtlasIndex();

                    if (graphic.Index == -1)
                    {
                        continue;
                    }

                    _vertices[0] = basePos + x * xTile + y * yTile; // Top Left
                    _vertices[1] = _vertices[0] + xTile; // Top Right
                    _vertices[2] = _vertices[1] + yTile; // Bottom Right
                    _vertices[3] = _vertices[0] + yTile; // Bottom Left

                    Vector2 baseUV = new Vector2((graphic.Index % 5) * 0.2f, 1 - (graphic.Index / 5) * 0.2f);

                    _uvs[0] = baseUV + new Vector2(0, -0.2f);
                    _uvs[1] = baseUV + new Vector2(0.2f, -0.2f);
                    _uvs[2] = baseUV + new Vector2(0.2f, 0);
                    _uvs[3] = baseUV;

                    if (graphic.Flip)
                    {
                        (_uvs[0], _uvs[1], _uvs[2], _uvs[3]) = (_uvs[1], _uvs[0], _uvs[3], _uvs[2]);
                    }

                    int baseVertex = topComp.GetVertexIndex();

                    topComp.AddVertices(_vertices, _uvs);

                    int cut = tile.CutDir();

                    int underVertex = AddUndersideVerts(underComp);

                    if (cut == 0)
                    {
                        topComp.AddTriangle(baseVertex, 0, 1, 2);
                        topComp.AddTriangle(baseVertex, 2, 3, 0);

                        underComp.AddTriangle(underVertex, 1, 0, 2);
                        underComp.AddTriangle(underVertex, 3, 2, 0);
                    }
                    else if (cut == -1)
                    {
                        topComp.AddTriangle(baseVertex, 1, 2, 3);
                        underComp.AddTriangle(underVertex, 2, 1, 3);
                    }
                    else if (cut == 1)
                    {
                        topComp.AddTriangle(baseVertex, 0, 2, 3);
                        underComp.AddTriangle(underVertex, 2, 0, 3);
                    }

                    if (y == 0 && cut == 0)
                    {
                        // Draw top edge
                        DrawTrim(trimComp, edgeComp, _vertices[0], _vertices[1]);
                    }

                    if (y == _tileHeight - 1)
                    {
                        // Draw bottom edge
                        DrawTrim(trimComp, edgeComp, _vertices[3], _vertices[2]);
                    }

                    if (_lFlat && x == 0)
                    {
                        // Draw left edge
                        DrawTrim(trimComp, edgeComp, _vertices[0], _vertices[3]);
                    }

                    if (_rFlat && x == _tileWidth - 1)
                    {
                        // Draw right edge
                        DrawTrim(trimComp, edgeComp, _vertices[2], _vertices[1]);
                    }
                }
            }
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

        public bool Intersect(RoofEdge other)
        {
            // Can only run this if tiles line up.

            float heightOffset = _height / _slope - other._height / other._slope;

            if (_slope != other._slope || Math.Abs(heightOffset - Mathf.Round(heightOffset)) > Bias)
            {
                return false;
            }

            float dir = Vector2.Angle(_xNormal, other._xNormal);

            if (Vector2.Dot(_xNormal, other._yNormal) < 0)
            {
                dir *= -1;
            }

            float notches = dir / 90;

            if (Math.Abs(Math.Round(notches) - notches) > Bias)
            {
                // Not at a 90 degree angle.
                return false;
            }

            int dirDiff = ((int)Math.Round(notches) + 4) % 4;

            Vector3 basePos = new Vector3(_tl.x, _height + (_tileHeight - 1) * _slope, _tl.y);
            Vector3 otherBasePos = new Vector3(other._tl.x, other._height + (other._tileHeight - 1) * _slope, other._tl.y);

            Vector3 xTile = new Vector3(_xNormal.x * TileSize, 0, _xNormal.y * TileSize);
            Vector3 yTile = new Vector3(_yNormal.x * -TileSize, -_slope, _yNormal.y * -TileSize);

            switch (dirDiff)
            {
                case 0:
                    {
                        // Roofs are parallel. If one roof is entirely over the other, remove the lower tiles.
                        // If the two roofs occupy the same space, select tiles that best represent both roofs and delete the tile on the other.

                        float xTileOffset = Vector2.Dot(_xNormal, other._tl - _tl) / TileSize;
                        float yTileOffset = Vector2.Dot(_yNormal, other._tl - _tl) / TileSize;

                        int xTO = Mathf.RoundToInt(xTileOffset);
                        int yTO = Mathf.RoundToInt(yTileOffset);

                        //if (xTO > _tileWidth)

                        int heightOffsetTiles = yTO + (_tileHeight - other._tileHeight);  // yTO difference at base

                        // How much above the other edge are we?
                        float heightDiff = (_height - other._height) + heightOffsetTiles * _slope;

                        int xFrom = Math.Max(0, xTO);
                        int xTo = Math.Min(_tileWidth - 1, xTO + other._tileWidth);
                        int yFrom = Math.Max(0, -yTO);
                        int yTo = Math.Min(_tileHeight - 1, other._tileHeight - yTO);
                        int xFromOther = Math.Max(0, -xTO);
                        int yFromOther = Math.Max(0, yTO);

                        if (Math.Abs(heightDiff) < Bias)
                        {
                            // Roofs are roughly the same height.
                            // Prefer tiles from one roof with a higher priority.

                            for (int y = yFrom, yO = yFromOther; y <= yTo; y++, yO++)
                            {
                                for (int x = xFrom, xO = xFromOther; x <= xTo; x++, xO++)
                                {
                                    ref RoofTile tile = ref _tiles[y * _tileWidth + x];
                                    ref RoofTile oTile = ref other._tiles[yO * other._tileWidth + xO];

                                    int prio1 = tile.OverlapPriority();
                                    int prio2 = tile.OverlapPriority();

                                    if (prio1 > prio2)
                                    {
                                        oTile.Delete();
                                    }
                                    else
                                    {
                                        tile.Delete();
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool thisHigher = heightDiff > 0;
                            
                            // Remove lower tiles
                            for (int y = yFrom, yO = yFromOther; y <= yTo; y++, yO++)
                            {
                                for (int x = xFrom, xO = xFromOther; x <= xTo; x++, xO++)
                                {
                                    if (x == 0 || x == _tileWidth - 1 || y == _tileHeight - 1 ||
                                        xO == 0 || xO == other._tileWidth - 1 || yO == other._tileWidth - 1)
                                    {
                                        // Don't remove overhangs due to vertical overlap.
                                        // Only remove them if their donor tiles disappear.
                                        continue;
                                    }

                                    ref RoofTile tile = ref _tiles[y * _tileWidth + x];
                                    ref RoofTile oTile = ref other._tiles[yO * other._tileWidth + xO];

                                    if (thisHigher)
                                    {
                                        oTile.Delete();

                                        // Does this remove an overhang?

                                        if (xO == 1)
                                        {
                                            // Removes left overhang
                                            other._tiles[yO * other._tileWidth].Delete();

                                            if (yO == other._tileHeight - 2)
                                            {
                                                // ... and bottom corner
                                                other._tiles[(yO + 1) * other._tileWidth].Delete();
                                            }
                                        }
                                        else if (xO == other._tileWidth - 2)
                                        {
                                            // Removes right overhang
                                            other._tiles[yO * other._tileWidth + xO + 1].Delete();

                                            if (yO == other._tileHeight - 2)
                                            {
                                                // ... and bottom corner
                                                other._tiles[(yO + 1) * other._tileWidth + xO + 1].Delete();
                                            }
                                        }
                                        else if (yO == other._tileHeight - 2)
                                        {
                                            other._tiles[(yO + 1) * other._tileWidth].Delete();
                                        }
                                    }
                                    else
                                    {
                                        tile.Delete();

                                        // Does this remove an overhang?

                                        if (x == 1)
                                        {
                                            // Removes left overhang
                                            _tiles[y * _tileWidth].Delete();

                                            if (y == _tileHeight - 2)
                                            {
                                                // ... and bottom corner
                                                _tiles[(y + 1) * _tileWidth].Delete();
                                            }
                                        }
                                        else if (x == _tileWidth - 2)
                                        {
                                            // Removes right overhang
                                            _tiles[yO * _tileWidth + x + 1].Delete();

                                            if (yO == _tileHeight - 2)
                                            {
                                                // ... and bottom corner
                                                _tiles[(y + 1) * _tileWidth + x + 1].Delete();
                                            }
                                        }
                                        else if (y == _tileHeight - 2)
                                        {
                                            _tiles[(y + 1) * _tileWidth].Delete();
                                        }
                                    }
                                }
                            }
                        }

                        return true;
                        //break;
                    }
                case 1:
                    {
                        // Other wall is 90 degrees from this one.
                        // This wall sees a diagonal intersection in form /
                        // The other sees \
                        break;
                    }
                case 2:
                    {
                        // Roofs are opposite. Try find where the roofs intersect. Remove whichever one is lower, and on the intersection line add a cut.
                        break;
                    }
                case 3:
                    {
                        // Other wall is -90 degrees from this one.
                        // This wall sees a diagonal intersection in form \
                        // The other sees /
                        break;
                    }

            }

            return false;
        }

        public void RemoveTilesUnder(IRoofType other)
        {
            // This method runs when a roof cannot be intersected, such as roofs with different slope or unusual shape.
            // A tile can only be removed if all four of its corners are under another roof.
            Vector3 basePos = new Vector3(_tl.x, _height + (_tileHeight - 1) * _slope, _tl.y);

            Vector3 xTile = new Vector3(_xNormal.x * TileSize, 0, _xNormal.y * TileSize);
            Vector3 yTile = new Vector3(_yNormal.x * -TileSize, -_slope, _yNormal.y * -TileSize);

            basePos -= xTile;

            int i = 0;
            for (int y = 0; y < _tileHeight; y++)
            {
                for (int x = 0; x < _tileWidth; x++, i++)
                {
                    Vector3 tl = basePos + x * xTile + y * yTile; // Top Left
                    Vector3 tr = tl + xTile; // Top Right
                    Vector3 br = tr + yTile; // Bottom Right
                    Vector3 bl = tl + yTile; // Bottom Left

                    float tlH = other.GetHeightAt(tl.x, tl.z);
                    if (tlH != float.PositiveInfinity && tl.y + Bias < tlH)
                    {
                        float trH = other.GetHeightAt(tr.x, tr.z);
                        if (trH != float.PositiveInfinity && tr.y + Bias < trH)
                        {
                            float brH = other.GetHeightAt(br.x, br.z);
                            if (brH != float.PositiveInfinity && br.y + Bias < brH)
                            {
                                float blH = other.GetHeightAt(bl.x, bl.z);
                                if (blH != float.PositiveInfinity && bl.y + Bias < blH)
                                {
                                    // Remove this tile.
                                    _tiles[i].Delete();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot.Roof
{
    public enum RoofEdgeEnd
    {
        Normal,
        Flat,
        FlatShort
    }

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
        private const float DiagonalScale = 0.7071067811865475f; // sqrt(2) / 2

        private const float AtlasUVYOff = 2f / 512f;
        private const float AtlasUVSize = 102f / 512f;

        private const float Bias = 0.0001f;

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

        private RoofEdgeEnd _lEnd;
        private RoofEdgeEnd _rEnd;

        // Roof Tiles
        private RoofTile[] _tiles;
        private int _tileWidth;
        private int _tileHeight;

        private float _tileSize;
        private int _overhangSize;
        private int _topOverhangSize;
        private bool _pagoda;

        private Vector3[] _vertices;
        private Vector2[] _uvs;

        private Vector3[] _edgeVertices;
        private Vector2[] _edgeUVs;

        private Vector3[] _underVertices;
        private Vector2[] _underUVs;

        private bool _isDiagonal;

        public RoofEdge(
            float height,
            float slope,
            Vector2 bl,
            Vector2 br,
            Vector2 tr,
            Vector2 tl,
            RoofEdgeEnd lEnd = RoofEdgeEnd.Normal,
            RoofEdgeEnd rEnd = RoofEdgeEnd.Normal,
            int topOverhangSize = 0,
            bool pagoda = false)
        {
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

            if (_yNormal.sqrMagnitude < 0.5f)
            {
                _yNormal = new Vector2(-_xNormal.y, _xNormal.x);
            }

            if (_xNormal.sqrMagnitude < 0.5f)
            {
                _xNormal = new Vector2(_yNormal.y, -_yNormal.x);
            }

            _bPc = Vector2.Dot(_yNormal, bl);
            _rPc = Vector2.Dot(_xNormal, br);
            _tPc = Vector2.Dot(_yNormal, tr);
            _lPc = Vector2.Dot(_xNormal, tl);

            _lEnd = lEnd;
            _rEnd = rEnd;

            _vertices = new Vector3[5];
            _uvs = new Vector2[5];

            _edgeVertices = new Vector3[4];
            _edgeUVs = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
            };

            _underVertices = new Vector3[5];
            _underUVs = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0.5f, 0.5f),
            };

            // Don't really expect anything except cardinal and 45 degrees, so this works.
            _isDiagonal = Math.Abs(_xNormal.x) < 0.9f && Math.Abs(_xNormal.y) < 0.9f;

            if (_isDiagonal)
            {
                _tileSize = DiagonalScale * TileSize;
                _slope *= DiagonalScale;
                _overhangSize = 2;
            }
            else
            {
                _tileSize = TileSize;
                _overhangSize = 1;
            }

            _topOverhangSize = topOverhangSize;
            _pagoda = pagoda;

            if (pagoda)
            {
                _overhangSize += 2;
            }

            _tileWidth = Mathf.RoundToInt(xDist / _tileSize) + (_lEnd == RoofEdgeEnd.FlatShort ? 0 : _overhangSize) + (_rEnd == RoofEdgeEnd.FlatShort ? 0 : _overhangSize);
            _tileHeight = Mathf.RoundToInt(yDist / _tileSize) + _overhangSize;
            _tiles = new RoofTile[_tileWidth * _tileHeight];

            InitTiles();
        }

        private void InitTiles()
        {
            int mid = 0;

            if (_rEnd == RoofEdgeEnd.Normal && _lEnd == RoofEdgeEnd.Normal)
            {
                mid = _tileWidth / 2;
            }
            else if (_lEnd == RoofEdgeEnd.Normal)
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

                    if (_lEnd == RoofEdgeEnd.Normal && x < mid)
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
                                tile = new RoofTile(RoofTileBase.Empty);
                            }

                            continue;
                        }
                    }

                    if (_rEnd == RoofEdgeEnd.Normal && x >= mid)
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
                                tile = new RoofTile(RoofTileBase.Empty);
                            }

                            continue;
                        }
                    }

                    tile = new RoofTile(y == 0 ? RoofTileBase.Top : RoofTileBase.Normal);
                }
            }
        }

        private float GetPagodaSlopeAt(int yTile)
        {
            // At yTile = _tileHeight this should be 0.
            // At yTile = 2 the roof should intersect with a normal roof at _slope
            // At yTile = _tileHeight - _overhangSize the height should be 0.
            // For the two top yTile, this overshoots a little.

            float fromBottom = _tileHeight - yTile;
            float heightIntersect = (_tileHeight - (_overhangSize + 2)) * _slope;
            float intersectY = _tileHeight - 2;

            // Expected height when fromBottom is 0 = 0
            // Expected height when fromBottom is (tileHeight - 2) = heightIntersect

            // height = a * y^2
            float a = heightIntersect / (intersectY * intersectY);

            // dy(height) = 2a * y;
            return 2 * a * fromBottom;

            // The corners have an additional slope modifier (that also affects the x direction) but that's not supported right now.
        }

        private float GetMaxHeight()
        {
            if (_pagoda)
            {
                // Height of the roof without the top two tiles, then add them on as they overshoot.
                float baseSlope = (_tileHeight - (_overhangSize + 2)) * _slope;

                return baseSlope + GetPagodaSlopeAt(0) + GetPagodaSlopeAt(1);
            }
            else
            {
                return (_tileHeight - _overhangSize) * _slope;
            }
        }

        private void DrawTrim(LotArchitectureMeshComponent trimComp, LotArchitectureMeshComponent edgeComp, Vector3 from, Vector3 to)
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

        private int AddUndersideVerts(LotArchitectureMeshComponent underComp)
        {
            var off = new Vector3(0, -ThicknessTotal, 0);
            int index = underComp.GetVertexIndex();

            _underVertices[0] = _vertices[0] + off;
            _underVertices[1] = _vertices[1] + off;
            _underVertices[2] = _vertices[2] + off;
            _underVertices[3] = _vertices[3] + off;
            _underVertices[4] = _vertices[4] + off;

            underComp.AddVertices(_underVertices, _underUVs);

            return index;
        }

        public void GenerateGeometry(RoofGeometryCollection geo)
        {
            var trimComp = geo.RoofTrim.Component;
            var edgeComp = geo.RoofEdges.Component;
            var topComp = geo.RoofTop.Component;
            var underComp = geo.RoofUnder.Component;

            Vector3 basePos = new Vector3(_tl.x, _height + GetMaxHeight() + ThicknessTop, _tl.y);

            Vector3 xTile = new Vector3(_xNormal.x * _tileSize, 0, _xNormal.y * _tileSize);
            Vector3 yTile = new Vector3(_yNormal.x * -_tileSize, -_slope, _yNormal.y * -_tileSize);

            if (_lEnd != RoofEdgeEnd.FlatShort)
            {
                basePos -= xTile * _overhangSize;
            }

            int i = 0;
            float yBase = 0;
            for (int y = 0; y < _tileHeight; y++)
            {
                if (_pagoda)
                {
                    yTile.y = -GetPagodaSlopeAt(y);
                }

                for (int x = 0; x < _tileWidth; x++, i++)
                {
                    ref RoofTile tile = ref _tiles[i];

                    AtlasIndex graphic = tile.GetAtlasIndex();

                    if (graphic.Index == -1)
                    {
                        continue;
                    }

                    _vertices[0] = basePos + x * xTile + y * new Vector3(yTile.x, 0, yTile.z); // Top Left
                    _vertices[0].y += yBase;
                    _vertices[1] = _vertices[0] + xTile; // Top Right
                    _vertices[2] = _vertices[1] + yTile; // Bottom Right
                    _vertices[3] = _vertices[0] + yTile; // Bottom Left

                    if (_pagoda)
                    {
                        _vertices[0].y += PagodaCornerOffset(x);
                        _vertices[1].y += PagodaCornerOffset(x + 1);
                        _vertices[2].y += PagodaCornerOffset(x + 1);
                        _vertices[3].y += PagodaCornerOffset(x);
                    }

                    _vertices[4] = (_vertices[0] + _vertices[1] + _vertices[2] + _vertices[3]) / 4; // Center

                    Vector2 baseUV = new Vector2((graphic.Index % 5) * AtlasUVSize, 1 - ((graphic.Index / 5) * AtlasUVSize + AtlasUVYOff));

                    _uvs[0] = baseUV + new Vector2(0, -AtlasUVSize);
                    _uvs[1] = baseUV + new Vector2(AtlasUVSize, -AtlasUVSize);
                    _uvs[2] = baseUV + new Vector2(AtlasUVSize, 0);
                    _uvs[3] = baseUV;
                    _uvs[4] = baseUV + new Vector2(AtlasUVSize / 2, -AtlasUVSize / 2);

                    if (graphic.Flip)
                    {
                        (_uvs[0], _uvs[1], _uvs[2], _uvs[3]) = (_uvs[1], _uvs[0], _uvs[3], _uvs[2]);
                    }

                    int baseVertex = topComp.GetVertexIndex();

                    topComp.AddVertices(_vertices, _uvs);

                    RoofCut cut = tile.Cuts();

                    int underVertex = AddUndersideVerts(underComp);

                    if (cut == 0)
                    {
                        topComp.AddTriangle(baseVertex, 0, 1, 2);
                        topComp.AddTriangle(baseVertex, 2, 3, 0);

                        underComp.AddTriangle(underVertex, 1, 0, 2);
                        underComp.AddTriangle(underVertex, 3, 2, 0);
                    }
                    else
                    {
                        // 4 triangle cut.
                        if ((cut & RoofCut.Top) == 0)
                        {
                            topComp.AddTriangle(baseVertex, 0, 1, 4);
                            underComp.AddTriangle(underVertex, 1, 0, 4);
                        }

                        if ((cut & RoofCut.Right) == 0)
                        {
                            topComp.AddTriangle(baseVertex, 1, 2, 4);
                            underComp.AddTriangle(underVertex, 2, 1, 4);
                        }

                        if ((cut & RoofCut.Bottom) == 0)
                        {
                            topComp.AddTriangle(baseVertex, 2, 3, 4);
                            underComp.AddTriangle(underVertex, 3, 2, 4);
                        }

                        if ((cut & RoofCut.Left) == 0)
                        {
                            topComp.AddTriangle(baseVertex, 3, 0, 4);
                            underComp.AddTriangle(underVertex, 0, 3, 4);
                        }
                    }

                    if (y == 0 && (cut & RoofCut.Top) == 0)
                    {
                        // Draw top edge
                        DrawTrim(trimComp, edgeComp, _vertices[1], _vertices[0]);
                    }

                    if (y == _tileHeight - 1 && (cut & RoofCut.Bottom) == 0)
                    {
                        // Draw bottom edge
                        DrawTrim(trimComp, edgeComp, _vertices[3], _vertices[2]);
                    }

                    if (_lEnd != RoofEdgeEnd.Normal && x == 0 && (cut & RoofCut.Left) == 0)
                    {
                        // Draw left edge
                        DrawTrim(trimComp, edgeComp, _vertices[0], _vertices[3]);
                    }

                    if (_rEnd != RoofEdgeEnd.Normal && x == _tileWidth - 1 && (cut & RoofCut.Right) == 0)
                    {
                        // Draw right edge
                        DrawTrim(trimComp, edgeComp, _vertices[2], _vertices[1]);
                    }
                }
                yBase += yTile.y;
            }
        }

        public float PagodaCornerOffset(float x)
        {
            int cornerSize = Math.Min(4, Math.Max(0, _tileHeight - 4));

            if (x < cornerSize && _lEnd == RoofEdgeEnd.Normal)
            {
                float scale = cornerSize - x;
                return scale * scale * (1 / 16f) * _slope;
            }

            if (x > _tileWidth - cornerSize && _rEnd == RoofEdgeEnd.Normal)
            {
                float scale = x - (_tileWidth - cornerSize);
                return scale * scale * (1 / 16f) * _slope;
            }

            return 0;
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

            if ((_lEnd == RoofEdgeEnd.Normal && dotX - _lPc < yDist) || (_rEnd == RoofEdgeEnd.Normal && _rPc - dotX < yDist))
            {
                // If the edges are not flat, also cut off top 45 degrees from left and right.
                return float.PositiveInfinity;
            }

            if (_pagoda)
            {
                float fromBottom = (yDist / _tileSize) + _overhangSize;
                float heightIntersect = (_tileHeight - (_overhangSize + 2)) * _slope;
                float intersectY = (_tileHeight - 2);

                // height = a * y^2
                float a = heightIntersect / (intersectY * intersectY);

                return a * fromBottom * fromBottom + _height;
            }
            else
            {
                return yDist * (_slope / _tileSize) + _height;
            }
        }

        private void CutUsingTile(in RoofTile oTile, int x, int y, int dir)
        {
            ref RoofTile tile = ref _tiles[y * _tileWidth + x];

            tile.Cut(oTile, dir);
        }

        private bool IsOverhang(int x, int y)
        {
            return (x < _overhangSize && _lEnd != RoofEdgeEnd.FlatShort) || (x >= _tileWidth - _overhangSize && _rEnd != RoofEdgeEnd.FlatShort) || y >= _tileHeight - _overhangSize || y < _topOverhangSize;
        }

        private bool IsOverhangX(int x, int y)
        {
            return (x < _overhangSize && _lEnd != RoofEdgeEnd.FlatShort) || (x >= _tileWidth - _overhangSize && _rEnd != RoofEdgeEnd.FlatShort);
        }

        public bool Intersect(RoofEdge other)
        {
            // Can only run this if tiles line up.

            float heightOffset = _height / _slope - other._height / other._slope;

            if (_slope != other._slope || Math.Abs(heightOffset - Mathf.Round(heightOffset)) > Bias || _tileSize != other._tileSize || _pagoda || other._pagoda)
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

            float xTileOffset = Vector2.Dot(_xNormal, other._tl - _tl) / _tileSize;
            float yTileOffset = Vector2.Dot(_yNormal, other._tl - _tl) / _tileSize;

            int xTO = Mathf.RoundToInt(xTileOffset);
            int yTO = Mathf.RoundToInt(yTileOffset);

            switch (dirDiff)
            {
                case 0:
                    {
                        // Roofs are parallel. If one roof is entirely over the other, remove the lower tiles.
                        // If the two roofs occupy the same space, select tiles that best represent both roofs and delete the tile on the other

                        xTO += (_lEnd == RoofEdgeEnd.FlatShort ? 0 : _overhangSize) - (other._lEnd == RoofEdgeEnd.FlatShort ? 0 : other._overhangSize); // Account for left extents

                        int heightOffsetTiles = yTO + (_tileHeight - other._tileHeight);  // yTO difference at base

                        // How much above the other edge are we?
                        float heightDiff = (_height - other._height) + heightOffsetTiles * _slope;

                        int xFrom = Math.Max(0, xTO);
                        int xTo = Math.Min(_tileWidth - 1, xTO + other._tileWidth - 1);
                        int yFrom = Math.Max(0, -yTO);
                        int yTo = Math.Min(_tileHeight - 1, (other._tileHeight - 1) - yTO);
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
                                    int prio2 = oTile.OverlapPriority();

                                    if (prio1 > prio2)
                                    {
                                        tile.AddIntersectionsFrom(oTile);
                                        oTile.Delete();
                                    }
                                    else
                                    {
                                        oTile.AddIntersectionsFrom(tile);
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
                                    ref RoofTile tile = ref _tiles[y * _tileWidth + x];
                                    ref RoofTile oTile = ref other._tiles[yO * other._tileWidth + xO];

                                    if (thisHigher)
                                    {
                                        if (!IsOverhang(x, y) || (_height == other._height && other.IsOverhangX(xO, yO)))
                                        {
                                            // Only real roof can remove lower tiles if they're on a different level.
                                            other.CutUsingTile(tile, xO, yO, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (!other.IsOverhang(xO, yO) || (_height == other._height && IsOverhangX(x, y)))
                                        {
                                            CutUsingTile(oTile, x, y, 0);
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
                case 1:
                case 3:
                    {
                        if (dirDiff == 3)
                        {
                            // It's negative 90.
                            // Flip this and other.
                            (xTO, yTO) = (-yTO, xTO);
                        }

                        RoofEdge left = (dirDiff == 3) ? other : this;
                        RoofEdge right = (dirDiff == 3) ? this : other;

                        xTO += left._lEnd == RoofEdgeEnd.FlatShort ? 0 : left._overhangSize; // Add left edge overflow of left
                        yTO += right._lEnd == RoofEdgeEnd.FlatShort ? 0 : right._overhangSize; // Subtraft left edge of right

                        // Other wall is 90 degrees from this one.
                        // Left wall sees a diagonal intersection in form /
                        // The right sees \

                        int xFrom = Math.Max(0, xTO - right._tileHeight);
                        int xTo = Math.Min(left._tileWidth - 1, xTO - 1);
                        int yFrom = Math.Max(0, -yTO);
                        int yTo = Math.Min(left._tileHeight - 1, (right._tileWidth - 1) - yTO);
                        int xFromOther = Math.Max(0, yTO);
                        int yFromOther = Math.Min(right._tileHeight - 1, xTO - 1);

                        int heightNotches = Mathf.RoundToInt(left._height / left._slope);
                        int oHeightNotches = Mathf.RoundToInt(right._height / right._slope);

                        for (int y = yFrom, xO = xFromOther; y <= yTo; y++, xO++)
                        {
                            for (int x = xFrom, yO = yFromOther; x <= xTo; x++, yO--)
                            {
                                int tileHeight = heightNotches + (left._tileHeight - 1) - y;
                                int tileHeightOther = oHeightNotches + (right._tileHeight - 1) - yO;

                                ref RoofTile tile = ref left._tiles[y * left._tileWidth + x];
                                ref RoofTile oTile = ref right._tiles[yO * right._tileWidth + xO];

                                if (tileHeight < tileHeightOther)
                                {
                                    if (!right.IsOverhang(xO, yO))
                                    {
                                        left.CutUsingTile(oTile, x, y, 3);
                                    }
                                }
                                else
                                {
                                    if (tileHeight == tileHeightOther)
                                    {
                                        // The fun one.

                                        bool lOverhang = left.IsOverhang(x, y);
                                        bool rOverhang = right.IsOverhang(xO, yO);

                                        if (lOverhang == rOverhang)
                                        {
                                            // Cut the top left of the left edge into the right edge.
                                            RoofCut cutFromL = (tile.Cuts() & RoofCut.TopLeft) | RoofCut.BottomRight;
                                            oTile.Cut(cutFromL, 1);

                                            // Cut the top right of the right edge into the left edge.

                                            RoofCut cutFromR = (oTile.Cuts() & RoofCut.TopRight) | RoofCut.BottomLeft;
                                            tile.Cut(cutFromR, 3);
                                        }

                                        // Only add the intersection graphics to the left face if the right isn't fully cut out.
                                        if ((oTile.Cuts() & RoofCut.TopRight) != RoofCut.TopRight)
                                        {
                                            tile.AddIntersection(RoofTileIntersection.Left);

                                            if (y > 0)
                                            {
                                                left._tiles[(y - 1) * left._tileWidth + x].AddIntersection(RoofTileIntersection.SmallLeft);

                                                if (x < left._tileWidth - 1)
                                                {
                                                    left._tiles[(y - 1) * left._tileWidth + x + 1].AddIntersection(RoofTileIntersection.SmallRight);
                                                }
                                            }

                                            if (x > 0 && y == yTo)
                                            {
                                                left._tiles[y * left._tileWidth + x - 1].AddIntersection(RoofTileIntersection.SmallLeft);
                                            }
                                        }

                                        // Only add the intersection graphics to the right face if the left isn't fully cut out.
                                        if ((tile.Cuts() & RoofCut.TopLeft) != RoofCut.TopLeft)
                                        {
                                            oTile.AddIntersection(RoofTileIntersection.Right);

                                            if (yO > 0)
                                            {
                                                right._tiles[(yO - 1) * right._tileWidth + xO].AddIntersection(RoofTileIntersection.SmallRight);

                                                if (xO > 0)
                                                {
                                                    right._tiles[(yO - 1) * right._tileWidth + xO - 1].AddIntersection(RoofTileIntersection.SmallLeft);
                                                }
                                            }

                                            if (xO < right._tileWidth - 1)
                                            {
                                                right._tiles[yO * right._tileWidth + xO + 1].AddIntersection(RoofTileIntersection.SmallRight);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!left.IsOverhang(x, y))
                                        {
                                            right.CutUsingTile(tile, xO, yO, 1);
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        // Roofs are opposite. Try find where the roofs intersect. Remove whichever one is lower, and on the intersection line add a cut.

                        // Top lefts are flipped for each roof. Coordinates need to be flipped to be used on each other.

                        // On offsets between inverted axis, we also need to count the extra tiles.
                        xTO += (_lEnd == RoofEdgeEnd.FlatShort ? 0 : _overhangSize) + (other._rEnd == RoofEdgeEnd.FlatShort ? 0 : other._overhangSize); // Add this left and other right extent

                        int xFrom = Math.Max(0, xTO - other._tileWidth);
                        int xTo = Math.Min(_tileWidth - 1, xTO - 1);
                        int yFrom = Math.Max(0, -yTO - other._tileHeight);
                        int yTo = Math.Min(_tileHeight - 1, -yTO - 1);
                        int xFromOther = Math.Min(other._tileWidth - 1, xTO - 1);
                        int yFromOther = Math.Min(other._tileHeight - 1, -yTO - 1);

                        int heightNotches = Mathf.RoundToInt(_height / _slope);
                        int oHeightNotches = Mathf.RoundToInt(other._height / other._slope);

                        for (int y = yFrom, yO = yFromOther; y <= yTo; y++, yO--)
                        {
                            int tileHeight = heightNotches + (_tileHeight - 1) - y;
                            int tileHeightOther = oHeightNotches + (other._tileHeight - 1) - yO;

                            for (int x = xFrom, xO = xFromOther; x <= xTo; x++, xO--)
                            {
                                ref RoofTile tile = ref _tiles[y * _tileWidth + x];
                                ref RoofTile oTile = ref other._tiles[yO * other._tileWidth + xO];

                                if (tileHeight < tileHeightOther)
                                {
                                    if (!other.IsOverhang(xO, yO) || (_height == other._height && IsOverhangX(x, y)))
                                    {
                                        CutUsingTile(oTile, x, y, dirDiff);
                                    }

                                    if (tileHeightOther == tileHeight + 1 && (tile.GetBaseCut() & RoofCut.Top) == 0 && y > 0)
                                    {
                                        if (xO > 0)
                                        {
                                            other._tiles[yO * other._tileWidth + xO - 1].AddIntersection(RoofTileIntersection.SmallLeft);
                                        }

                                        oTile.AddIntersection(RoofTileIntersection.Bottom);

                                        if (xO < other._tileWidth - 1)
                                        {
                                            other._tiles[yO * other._tileWidth + xO + 1].AddIntersection(RoofTileIntersection.SmallRight);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!IsOverhang(x, y) || (_height == other._height && other.IsOverhangX(xO, yO)))
                                    {
                                        other.CutUsingTile(tile, xO, yO, dirDiff);
                                    }

                                    if (tileHeight == tileHeightOther + 1 && (oTile.GetBaseCut() & RoofCut.Top) == 0 && yO > 0)
                                    {
                                        if (x > 0)
                                        {
                                            _tiles[y * _tileWidth + x - 1].AddIntersection(RoofTileIntersection.SmallLeft);
                                        }

                                        tile.AddIntersection(RoofTileIntersection.Bottom);

                                        if (x < _tileWidth - 1)
                                        {
                                            _tiles[y * _tileWidth + x + 1].AddIntersection(RoofTileIntersection.SmallRight);
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }

            }

            return true;
        }

        public void RemoveTilesUnder(IRoofType other)
        {
            // This method runs when a roof cannot be intersected, such as roofs with different slope or unusual shape.
            // A tile can only be removed if all four of its corners are under another roof.
            Vector3 basePos = new Vector3(_tl.x, _height + (_tileHeight - 1) * _slope, _tl.y);

            Vector3 xTile = new Vector3(_xNormal.x * _tileSize, 0, _xNormal.y * _tileSize);
            Vector3 yTile = new Vector3(_yNormal.x * -_tileSize, -_slope, _yNormal.y * -_tileSize);

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
using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenTS2.Scenes.Lot
{
    [StructLayout(LayoutKind.Explicit)]
    struct IntToFloat
    {
        [FieldOffset(0)] private float f;
        [FieldOffset(0)] private uint i;
        public static float Convert(uint value)
        {
            return new IntToFloat { i = value }.f;
        }
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class LotTerrainComponent : AssetReferenceComponent
    {
        private LotTexturesAsset _textures;
        private _3DArrayAsset<float> _elevationData;
        private _2DArrayAsset<byte>[] _blendMaskData;
        private int _baseLevel;

        private Texture2D _baseTexture;
        private Texture2D _blendBitmap;
        private RenderTexture _blendTextures;
        private Texture2DArray _blendMasks;

        private Mesh _terrainMesh;
        private Material _material;

        public void CreateFromTerrainAssets(LotTexturesAsset textures, _3DArrayAsset<float> elevation, _2DArrayAsset<byte>[] blend, int baseLevel)
        {
            // Some constraints...
            // Heightmap size must match size in textures, and all blend sizes must be 4x (-1) on both axis.
            // Blend textures must have the same count as the # of blend masks.

            if (textures.Width != elevation.Width - 1 || textures.Height != elevation.Height - 1)
            {
                throw new InvalidOperationException("Size mismatch between heightmap and LTTX");
            }

            if (textures.BlendTextures.Length != blend.Length)
            {
                throw new InvalidOperationException("Blend texture count mismatch between LOTG and LTTX");
            }

            foreach (var mask in blend)
            {
                if (mask.Width != elevation.Width * 4 - 3 || mask.Height != elevation.Height * 4 - 3)
                {
                    throw new InvalidOperationException("Size mismatch between mask and heightmap");
                }
            }

            _textures = textures;
            _elevationData = elevation;
            _blendMaskData = blend;
            _baseLevel = baseLevel;

            LoadLotTextures();
            GenerateTerrainVertices();
            GenerateTerrainIndices();
            LoadAllBlendMasks();
            GenerateBlendBitmap();

            PrepareMesh();
            BindMaterialAndTextures();
        }

        private (ScenegraphTextureAsset color, ScenegraphTextureAsset bump) LoadTexture(ContentProvider contentProvider, string name)
        {
            var result = (
                contentProvider.GetAsset<ScenegraphTextureAsset>(new ResourceKey($"{name}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR)),
                contentProvider.GetAsset<ScenegraphTextureAsset>(new ResourceKey($"{name}-bump_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR))
            );

            AddReference(result.Item1, result.Item2);

            return result;
        }

        private void LoadLotTextures()
        {
            var contentProvider = ContentProvider.Get();

            var baseTexture = LoadTexture(contentProvider, _textures.BaseTexture);

            _baseTexture = baseTexture.color.GetSelectedImageAsUnityTexture(contentProvider);

            var blendTextures = new (Texture2D color, Texture2D bump)[_textures.BlendTextures.Length];

            int maxWidth = 32;
            int maxHeight = 32;

            int i = 0;
            foreach (string name in _textures.BlendTextures)
            {
                var textures = LoadTexture(contentProvider, name);
                blendTextures[i] = (textures.color?.GetSelectedImageAsUnityTexture(contentProvider) ?? Texture2D.whiteTexture, textures.bump?.GetSelectedImageAsUnityTexture(contentProvider) ?? Texture2D.whiteTexture);

                maxWidth = Math.Max(Math.Max(maxWidth, blendTextures[i].color?.width ?? 0), blendTextures[i].bump?.width ?? 0);
                maxHeight = Math.Max(Math.Max(maxHeight, blendTextures[i].color?.height ?? 0), blendTextures[i].bump?.height ?? 0);

                i++;
            }

            int mips = (int)Math.Min(Math.Log(maxWidth, 2.0), Math.Log(maxHeight, 2.0));

            if (_blendTextures == null)
            {
                // We lose compression since we might need to generate higher sized mips, as all mips need to be equal in texture arrays.
                _blendTextures = new RenderTexture(new RenderTextureDescriptor(maxWidth, maxHeight, RenderTextureFormat.BGRA32, 0, mips));
                _blendTextures.useMipMap = true;
                _blendTextures.dimension = TextureDimension.Tex2DArray;
                _blendTextures.volumeDepth = _blendMaskData.Length;
                _blendTextures.wrapMode = TextureWrapMode.Repeat;
            }

            // TODO: bump

            i = 0;
            foreach (var tex in blendTextures)
            {
                float scaleW = maxWidth / (float)tex.color.width;
                float scaleH = maxHeight / (float)tex.color.height;

                Graphics.Blit(tex.color, _blendTextures, new Vector2(scaleW, scaleH), Vector2.zero, 0, i++);
            }
        }

        private void LoadAllBlendMasks()
        {
            int maskWidth = (_elevationData.Width * 4 - 3);
            int maskHeight = (_elevationData.Height * 4 - 3);

            if (_blendMasks == null)
            {
                _blendMasks = new Texture2DArray(maskWidth, maskHeight, _blendMaskData.Length, TextureFormat.R8, false);
                _blendMasks.wrapMode = TextureWrapMode.Clamp;
            }

            int i = 0;
            foreach (var mask in _blendMaskData)
            {
                _blendMasks.SetPixelData(mask.Data, 0, i++);
            }

            _blendMasks.Apply();
        }

        private const float TerrainGridSize = 1f;
        private const float TerrainOffset = 0f;

        private void EnsureMesh()
        {
            if (_terrainMesh == null)
            {
                _terrainMesh = new Mesh();

                _material = new Material(Shader.Find("OpenTS2/Terrain"));
            }
        }

        private void GenerateTerrainVertices()
        {
            int width = _elevationData.Width;
            int height = _elevationData.Height;

            float[] data = _elevationData.Data[-_baseLevel];

            int size = data.Length + (width - 1) * (height - 1);

            var vertices = new Vector3[size];
            var uvs = new Vector2[size];

            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++, i++)
                {
                    vertices[i] = new Vector3(x * TerrainGridSize, data[i] + TerrainOffset, y * TerrainGridSize);
                    uvs[i] = new Vector2(x, y);
                }
            }

            // Now for midpoints, which are an average of the 4 corners.

            int mWidth = width - 1;
            int mHeight = height - 1;

            float fx = 0.5f;
            for (int x = 0; x < mWidth; x++, fx++)
            {
                float fy = 0.5f;
                int vertPos = x * height;
                for (int y = 0; y < mHeight; y++, fy++, i++, vertPos++)
                {
                    float average = (data[vertPos] + data[vertPos + height] + data[vertPos + 1] + data[vertPos + height + 1]) / 4f;

                    vertices[i] = new Vector3(fx * TerrainGridSize, average + TerrainOffset, fy * TerrainGridSize);
                    uvs[i] = new Vector2(fx, fy);
                }
            }

            EnsureMesh();

            _terrainMesh.SetVertices(vertices);
            _terrainMesh.SetUVs(0, uvs);
        }

        private void GenerateTerrainIndices()
        {
            // When flooring is implemented, this should avoid generating terrain triangles where there is flooring.
            // Should also respect diagonals.

            // For now, just fill every tile.

            int width = _elevationData.Width - 1;
            int height = _elevationData.Height - 1;

            var indices = new int[width * height * 12];

            int midOffset = _elevationData.Width * _elevationData.Height;

            int i = 0;
            int vi = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++, vi++)
                {
                    // Triangles wrap around a midpoint;
                    int mid = vi + midOffset - x;

                    indices[i] = vi;
                    indices[i + 1] = vi + 1;
                    indices[i + 2] = mid;

                    indices[i + 3] = vi + 1;
                    indices[i + 4] = vi + height + 2;
                    indices[i + 5] = mid;

                    indices[i + 6] = vi + height + 2;
                    indices[i + 7] = vi + height + 1;
                    indices[i + 8] = mid;

                    indices[i + 9] = vi + height + 1;
                    indices[i + 10] = vi;
                    indices[i + 11] = mid;

                    i += 12;
                }

                vi++;
            }

            EnsureMesh();
            _terrainMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }

        private void PrepareMesh()
        {
            _terrainMesh.RecalculateNormals();
            _terrainMesh.RecalculateTangents();

            GetComponent<MeshFilter>().sharedMesh = _terrainMesh;
        }

        private void BindMaterialAndTextures()
        {
            var meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = _material;

            _material.SetTexture("_BaseTexture", _baseTexture);
            _material.SetTexture("_BlendBitmap", _blendBitmap);
            _material.SetTexture("_BlendTextures", _blendTextures);
            _material.SetTexture("_BlendMasks", _blendMasks);

            _material.SetVector("_InvLotSize", new Vector4(1f / (_elevationData.Width - 1), 1f / (_elevationData.Height - 1)));
        }

        private void GenerateBlendBitmap()
        {
            // The blend bitmap tells the shader what blend textures are present at each texel.
            // This helps reduce the number of textures that need to be sampled in fragment.
            // This could be done with a render to texture, as long as it happens whenever the blend masks change.

            int bitmapWidth = (_elevationData.Width * 4 - 3);
            int bitmapHeight = (_elevationData.Height * 4 - 3);

            var size = bitmapWidth * bitmapHeight;

            var blendData = new uint[size];

            int bitN = 0;
            foreach (var blend in _blendMaskData)
            {
                uint bit = 1u << bitN;

                for (int i = 0; i < size; i++)
                {
                    if (blend.Data[i] > 0)
                    {
                        blendData[i] |= bit;
                    }
                }

                bitN++;
            }

            var blendDataF = new float[size];

            for (int i = 0; i < size; i++)
            {
                blendDataF[i] = IntToFloat.Convert(blendData[i]);
            }

            if (_blendBitmap == null)
            {
                // This is RUint, but for some reason that is not a choice, so we cast in the shader.
                _blendBitmap = new Texture2D(bitmapWidth, bitmapHeight, TextureFormat.RFloat, false);
                _blendBitmap.filterMode = FilterMode.Point;
                _blendBitmap.wrapMode = TextureWrapMode.Clamp;
            }

            _blendBitmap.SetPixelData(blendDataF, 0);
            _blendBitmap.Apply();
        }
    }
}
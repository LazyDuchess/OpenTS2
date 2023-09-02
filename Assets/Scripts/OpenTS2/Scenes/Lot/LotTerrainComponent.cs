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

    public class LotTerrainComponent : AssetReferenceComponent
    {

        private LotTexturesAsset _textures;
        private _3DArrayAsset<float> _elevationData;
        private _2DArrayAsset<byte>[] _blendMaskData;
        private _2DArrayAsset<float> _waterHeightData;
        private _3DArrayAsset<Vector4<ushort>> _patternData;
        private int _baseLevel;

        private Texture2D _baseTexture;
        private Texture2D _blendBitmap;
        private RenderTexture _blendTextures;
        private Texture2DArray _blendMasks;

        private PatternMesh _terrain;
        private PatternMesh _water;

        public void CreateFromTerrainAssets(
            LotTexturesAsset textures,
            _3DArrayAsset<float> elevation,
            _2DArrayAsset<byte>[] blend,
            _2DArrayAsset<float> waterHeightmap,
            _3DArrayAsset<Vector4<ushort>> patterns,
            int baseLevel)
        {
            // Some constraints...
            // Heightmap size must match size in textures, and all blend sizes must be 4x (-1) on both axis.
            // Blend textures must have the same count as the # of blend masks.

            if (textures.Width != elevation.Width - 1 || textures.Height != elevation.Height - 1)
            {
                throw new InvalidOperationException("Size mismatch between elevation and LTTX");
            }

            if (waterHeightmap.Width != elevation.Width || waterHeightmap.Height != elevation.Height)
            {
                throw new InvalidOperationException("Size mismatch between elevation and water heightmap");
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
            _waterHeightData = waterHeightmap;
            _patternData = patterns;
            _blendMaskData = blend;
            _baseLevel = baseLevel;

            PrepareMeshes();

            LoadLotTextures();
            BuildTerrainMesh();
            LoadAllBlendMasks();
            GenerateBlendBitmap();

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

        private void PrepareMeshes()
        {
            _terrain = new PatternMesh(gameObject, "Terrain", new Material(Shader.Find("OpenTS2/Terrain")));
            _water = new PatternMesh(gameObject, "Water", new Material(Shader.Find("OpenTS2/Water")));
        }

        private static int GetFilledMask(ref Vector4<ushort> p)
        {
            return (p.w != 0 ? 1 : 0) | (p.z != 0 ? 2 : 0) | (p.y != 0 ? 4 : 0) | (p.x != 0 ? 8 : 0);
        }

        private void BuildTerrainMesh()
        {
            int width = _elevationData.Width;
            int height = _elevationData.Height;

            Vector3[] tileVertices = new Vector3[5];
            Vector2[] tileUVs = new Vector2[5];

            _terrain.Component.Clear();
            _water.Component.Clear();

            float[] data = _elevationData.Data[-_baseLevel];

            int size = width * height + (width - 1) * (height - 1);

            var vertices = new Vector3[size];
            var uvs = new Vector2[size];

            for (int x = 0, i = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++, i++)
                {
                    vertices[i] = new Vector3(x, data[i], y);
                    uvs[i] = new Vector2(x, y);
                }
            }

            // Now for midpoints, which are an average of the 4 corners.

            int mWidth = width - 1;
            int mHeight = height - 1;

            float fx = 0.5f;
            for (int x = 0, i = width * height; x < mWidth; x++, fx++)
            {
                float fy = 0.5f;
                int vertPos = x * height;
                for (int y = 0; y < mHeight; y++, fy++, i++, vertPos++)
                {
                    float average = (data[vertPos] + data[vertPos + height] + data[vertPos + 1] + data[vertPos + height + 1]) / 4f;

                    vertices[i] = new Vector3(fx, average, fy);
                    uvs[i] = new Vector2(fx, fy);
                }
            }

            // Now to actually build the indices.

            LotArchitectureMeshComponent terrainComp = _terrain.Component;
            LotArchitectureMeshComponent waterComp = _water.Component;

            terrainComp.AddVertices(vertices, uvs);

            int midOffset = _elevationData.Width * _elevationData.Height;

            Vector4<ushort>[] patterns = _patternData.Data[-_baseLevel];
            Vector4<ushort>[] poolPatterns = _patternData.Data[0];

            float[] water = _waterHeightData.Data;

            for (int x = 0, i = 0, pi = 0, vi = 0; x < mWidth; x++)
            {
                for (int y = 0; y < mHeight; y++, vi++, pi++)
                {
                    int filledMask = GetFilledMask(ref patterns[pi]) | GetFilledMask(ref poolPatterns[pi]);

                    if (filledMask != 15)
                    {
                        // Triangles wrap around a midpoint
                        int mid = midOffset - x;

                        if ((filledMask & 8) == 0)
                        {
                            terrainComp.AddTriangle(vi, 0, 1, mid);
                        }

                        if ((filledMask & 4) == 0)
                        {
                            terrainComp.AddTriangle(vi, 1, height + 1, mid);
                        }

                        if ((filledMask & 2) == 0)
                        {
                            terrainComp.AddTriangle(vi, height + 1, height, mid);
                        }

                        if ((filledMask & 1) == 0)
                        {
                            terrainComp.AddTriangle(vi, height, 0, mid);
                        }

                        // Does water appear here?

                        int waterIndex = y * width + x;

                        float e0 = water[waterIndex];
                        float e1 = water[waterIndex + 1];
                        float e2 = water[waterIndex + 1 + width];
                        float e3 = water[waterIndex + width];

                        // Does the water elevation exceed the terrain elevation at any of the corners?

                        if (e0 > vertices[vi].y || e1 > vertices[vi + height].y || e2 > vertices[vi + height + 1].y || e3 > vertices[vi + 1].y)
                        {
                            tileVertices[0] = new Vector3(x, e0, y);
                            tileVertices[1] = new Vector3(x + 1, e1, y);
                            tileVertices[2] = new Vector3(x + 1, e2, y + 1);
                            tileVertices[3] = new Vector3(x, e3, y + 1);
                            tileVertices[4] = new Vector3(x + 0.5f, (e0 + e1 + e2 + e3) / 4, y + 0.5f);

                            tileUVs[0] = new Vector2(y, x);
                            tileUVs[1] = new Vector2(y, x + 1);
                            tileUVs[2] = new Vector2(y + 1, x + 1);
                            tileUVs[3] = new Vector2(y + 1, x);
                            tileUVs[4] = new Vector2(y + 0.5f, x + 0.5f);

                            int waterBase = waterComp.GetVertexIndex();

                            waterComp.AddVertices(tileVertices, tileUVs);

                            waterComp.AddTriangle(waterBase, 1, 0, 4);
                            waterComp.AddTriangle(waterBase, 2, 1, 4);
                            waterComp.AddTriangle(waterBase, 3, 2, 4);
                            waterComp.AddTriangle(waterBase, 0, 3, 4);
                        }
                    }

                    i += 12;
                }

                vi++;
            }

            terrainComp.Commit();
            waterComp.Commit();
        }

        private void BindMaterialAndTextures()
        {
            Material material = _terrain.Component.Material;

            material.SetTexture("_BaseTexture", _baseTexture);
            material.SetTexture("_BlendBitmap", _blendBitmap);
            material.SetTexture("_BlendTextures", _blendTextures);
            material.SetTexture("_BlendMasks", _blendMasks);

            material.SetVector("_InvLotSize", new Vector4(1f / (_elevationData.Width - 1), 1f / (_elevationData.Height - 1)));

            _water.Component.EnableShadows(false);

            Material waterMaterial = _water.Component.Material;
            // Very, VERY temporary.

            waterMaterial.SetColor("_Color", new Color(0.4103774f, 0.4560846f, 1, 0.3333333f));
            waterMaterial.SetFloat("_ReflectionMultiplier", 0f);
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
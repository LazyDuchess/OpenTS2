Shader "OpenTS2/Terrain"
{
    Properties
    {
        _BaseTexture ("Texture", 2D) = "white" {}
        _BlendBitmap("Blend Bitmap", 2D) = "black" {}
        _BlendTextures("Blend Textures", 2DArray) = "" {}
        _BlendMasks("Blend Masks", 2DArray) = "" {}

        _InvLotSize("Inv Lot Size", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma require 2darray

        #include "UnityCG.cginc"

        struct Input {
            float2 uv_BaseTexture;
        };

        sampler2D _BaseTexture;
        sampler2D _BlendBitmap;
        UNITY_DECLARE_TEX2DARRAY(_BlendTextures);
        UNITY_DECLARE_TEX2DARRAY(_BlendMasks);

        float4 _BlendBitmap_TexelSize;
        float4 _BlendTextures_TexelSize;
        float2 _InvLotSize;

        uint4 gather(sampler2D samp, fixed2 uv, float4 texSize) {
            // A shame that this isn't natively supported...
            float2 texelLocation = frac(uv) * texSize.zw;

            float2 offset = round(frac(texelLocation)) * float2(2, 2) - float2(1, 1);
            float2 texelCenter = floor(texelLocation) + float2(0.5, 0.5);

            uint4 result = uint4(
                asuint(tex2D(samp, texelCenter * texSize.xy).x),
                asuint(tex2D(samp, (texelCenter + int2(offset.x, 0)) * texSize.xy).x),
                asuint(tex2D(samp, (texelCenter + int2(0, offset.y)) * texSize.xy).x),
                asuint(tex2D(samp, (texelCenter + offset) * texSize.xy).x)
            );

            return result;
        }

        void surf(Input i, inout SurfaceOutput o)
        {
            // Sample base texture
            fixed2 texUV = i.uv_BaseTexture / 4.0;
            fixed4 col = tex2D(_BaseTexture, texUV);

            // Sample the bitmap to check which blend textures need to be sampled.
            fixed2 maskUV = (i.uv_BaseTexture - fixed2(3.0/16.0, 3.0/16.0)) * _InvLotSize.xy;
            uint4 textureBits = gather(_BlendBitmap, maskUV, _BlendBitmap_TexelSize);

            // Combine the gather values
            uint mask = textureBits.x | textureBits.y | textureBits.z | textureBits.w;

            fixed2 uvScaled = texUV * _BlendTextures_TexelSize.zw;

            float2 dxUv = ddx(uvScaled);
            float2 dyUv = ddy(uvScaled);
            float maxDerivative = max(dot(dxUv, dxUv), dot(dyUv, dyUv));
            float level = 0.5 * log2(maxDerivative);

            // Only blend in the layers that are in the mask.
            [loop]
            while (mask != 0)
            {
                int bit = firstbitlow(mask);

                fixed4 blendCol = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendTextures, float3(texUV, float(bit)), level);
                float blendMask = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlendMasks, float3(maskUV, float(bit)), 0).x;

                col = lerp(col, blendCol, blendMask);

                mask &= ~(1 << bit);
            }

            o.Albedo = col;
        }
        ENDCG
    }
    Fallback "Diffuse"
}

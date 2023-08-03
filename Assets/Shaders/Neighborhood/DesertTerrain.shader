// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "OpenTS2/DesertTerrain"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Shore("Shore Texture", 2D) = "white" {}
        _ShoreMask("Shore Mask", 2D) = "black" {}
        _Variation1("Variation 1", 2D) = "white" {}
        _CliffTex("Cliff Texture", 2D) = "white" {}
        _Roughness("Roughness Texture", 2D) = "white" {}
        _Roughness1("Roughness Variation 1", 2D) = "white" {}
        _MatCap("MatCap", 2D) = "white" {}
        _LightVector("Light Vector", Vector) = (.33, .33, -.33, 0)
        _Ambient("Ambient", float) = 0
        _Subtract("Subtract", float) = 0
        _SeaLevel("Sea Level", float) = 0
        _TerrainWidth("Terrain Width", float) = 128
        _TerrainHeight("Terrain Height", float) = 128
        _ShadowMap("Shadow Map", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Assets/Shaders/opents2_common.cginc"
            #include "UnityCG.cginc"
            #include "opents2_terrain_common.cginc"

            struct appdata {
                TERRAIN_APPDATA;
            };

            struct v2f
            {
                TERRAIN_V2F;
                float hills : TEXCOORD6;
            };

            v2f vert(appdata v)
            {
                TERRAIN_VERT;
                o.hills = max(0, o.height - (_SeaLevel * 1.135));
                o.hills *= 0.12;
                o.hills = min(1, o.hills);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 rCol = tex2D(_Variation1, i.uv);
                fixed4 redHill = tex2D(_Roughness1, i.uv);
                col = lerp(col, rCol, min(1,i.roughness * 2));

                rCol = tex2D(_Roughness, i.uv);

                rCol = lerp(rCol, redHill, i.hills);

                col = lerp(col, rCol, pow(i.roughness, 2));

                fixed4 cliffCol = tex2D(_CliffTex, i.uv);

                fixed4 shadowMapCol = tex2D(_ShadowMap, i.shadowUv);
                i.matcapUv *= shadowMapCol.r;
                float shoreAmount = tex2D(_ShoreMask, i.shadowUv);
                shoreAmount = min(1,shoreAmount + i.color.b);

                fixed4 shoreCol = tex2D(_Shore, i.uv);

                col = lerp(col, shoreCol, shoreAmount);
                col = lerp(col, cliffCol, i.cliff);
                col *= tex2D(_MatCap, i.matcapUv);

                fixed4 seaColor = fixed4(0, 0, 0, 1);

                float seaAmount = pow(max(0,-(i.height - _SeaLevel)) * 0.02, 0.4);
                seaAmount = min(1, seaAmount);

                col = lerp(col, seaColor, seaAmount);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

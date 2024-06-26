// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "OpenTS2/ClassicTerrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shore("Shore Texture", 2D) = "white" {}
        _ShoreMask("Shore Mask", 2D) = "black" {}
        _Variation1("Variation 1", 2D) = "white" {}
        _Variation2("Variation 2", 2D) = "white" {}
        _CliffTex("Cliff Texture", 2D) = "white" {}
        _Roughness("Roughness Texture", 2D) = "white" {}
        _Roughness1("Roughness Variation 1", 2D) = "white" {}
        _Roughness2("Roughness Variation 2", 2D) = "white" {}
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
        Tags { "RenderType"="Opaque" }
        LOD 100

        Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "Assets/Shaders/opents2_common.cginc"
            #include "UnityCG.cginc"
            #include "opents2_terrain_common.cginc"
            #include "AutoLight.cginc"

            struct appdata {
                TERRAIN_APPDATA;
            };

            struct v2f
            {
                TERRAIN_V2F
            };

            v2f vert(appdata v)
            {
                TERRAIN_VERT
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 rCol = tex2D(_Roughness, i.uv);
                if (i.color.r >= 0.99)
                {
                    col = tex2D(_Variation1, i.uv);
                    rCol = tex2D(_Roughness1, i.uv);
                }
                if (i.color.g >= 0.99)
                {
                    col = tex2D(_Variation2, i.uv);
                    rCol = tex2D(_Roughness2, i.uv);
                }
                col = lerp(col, rCol, i.roughness);
                fixed4 cliffCol = tex2D(_CliffTex, i.uv);

                float realTimeShadows = SHADOW_ATTENUATION(i);
                float iShadowUv = i.matcapUv;
                /*
                if (iShadowUv < 0.35)
                    iShadowUv = 1.0;
                    else
                    iShadowUv = 0;*/

                fixed4 shadowMapCol = tex2D(_ShadowMap, i.shadowUv);
                i.matcapUv *= shadowMapCol.r;

                
                realTimeShadows = lerp(iShadowUv, 1, realTimeShadows);
                i.matcapUv *= realTimeShadows;
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
                return col;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}

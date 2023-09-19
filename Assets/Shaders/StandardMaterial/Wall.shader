Shader "OpenTS2/StandardMaterial/Wall"
{
    Properties
    {
        // Parameters common to all StandardMaterial shaders.
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _WallsDownTex ("Walls Down Texture", 2D) = "black" {}
        [NoScaleOffset] _MaskTex ("Mask Texture", 2D) = "white" {}
        _AlphaMultiplier ("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
        _BumpMap("Bump Map", 2D) = "white" {}
        _DiffuseCoefficient("Diffuse color coefficient", Color) = (1, 1, 1, 1)
        _InvLotSize("Inverse lot size", Vector) = (1, 1, 0, 0)

        // Parameters specific to AlphaCutOut.
        _AlphaCutOff ("AlphaCutOff", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "Queue"="AlphaTest" }
        Stencil {
                Ref 0
                Comp Always
                Pass Replace
            }
        CGPROGRAM
        #pragma surface surf Lambert addshadow fullforwardshadows alphatest:_AlphaCutOff vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;
        sampler2D _WallsDownTex;
        sampler2D _BumpMap;
        uniform float4 _BumpMap_TexelSize;
        float _AlphaMultiplier;

        float4 _DiffuseCoefficient;
        float4 _InvLotSize;

        struct Input
        {
            float2 uv_MainTex;
            float3 vertex;
        };

        float3 normalFromBumpMap(float2 coords, float intensity)
        {
            float3 graynorm = float3(0, 0, 1);
            float heightSampleCenter = tex2D(_BumpMap, coords).r * intensity;
            float heightSampleRight = tex2D(_BumpMap, coords + float2(_BumpMap_TexelSize.x, 0)).r * intensity;
            float heightSampleUp = tex2D(_BumpMap, coords + float2(0, _BumpMap_TexelSize.y)).r * intensity;
            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
            float sampleDeltaUp = heightSampleUp - heightSampleCenter;
            graynorm = cross(
                float3(1, 0, sampleDeltaRight),
                float3(0, 1, sampleDeltaUp));

            float3 bumpNormal = normalize(graynorm);
            return bumpNormal;
        }

        void cutWallsDown (inout appdata_full v) {
            fixed2 uv = fixed2(v.vertex.x, v.vertex.z) * _InvLotSize.xy;
            fixed wallDown = tex2Dlod(_WallsDownTex, fixed4(uv, 0.0, 0.0)).r;
            fixed offset = wallDown * v.texcoord1.y;

            v.vertex.y -= offset;
            v.texcoord.y += offset / 3.0;
        }

        void vert (inout appdata_full v) {
            #if !defined(UNITY_PASS_SHADOWCASTER)
            cutWallsDown(v);
            #else
            // A bit of a hack, since the same shader is used for depth prepass (forward rendering).
            // In the shadowmap shader, this should be assigned.
            if (unity_LightShadowBias.y == 0.0) {
                cutWallsDown(v);
            }
            #endif
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = _DiffuseCoefficient;
            float2 uv = IN.uv_MainTex;
            c *= tex2D (_MainTex, uv);
            c.a *= tex2D (_MaskTex, uv).r;
            c.a *= _AlphaMultiplier;

            float3 bumpNormal = normalFromBumpMap(uv, 5);

            o.Normal = bumpNormal;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
    }
    FallBack "Diffuse"
}

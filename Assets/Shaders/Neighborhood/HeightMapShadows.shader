Shader "OpenTS2/HeightMapShadows"
{
    Properties
    {
        _MainTex("Height Map", 2D) = "white" {}
        _LightVector("Light Vector", Vector) = (.33, .33, -.33, 0)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _NormalMap;
                float4 _MainTex_ST;
                float4 _LightVector;


                float getShade(float2 coord, float increments, float maxDistance)
                {
                    float shadow = 1;
                    float beginHeightSample = tex2D(_MainTex, coord).x;
                    float3 beginPosition = float3(coord.x, beginHeightSample, coord.y);
                    for (float i = 0; i < maxDistance; i += increments)
                    {
                        float3 currentPosition = beginPosition - _LightVector * i;
                        float2 currentCoord = float2(currentPosition.x, currentPosition.z);
                        float currentHeight = tex2D(_MainTex, currentCoord).x;
                        shadow *= clamp((currentPosition.y / currentHeight), 0., 1.);
                    }
                    return shadow;
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    //fixed4 col = tex2D(_MainTex, i.uv);
                float shade = getShade(i.uv, 0.002, 1.0);
                fixed4 col = fixed4(shade, shade, shade, 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        }
}
Shader "OpenTS2/ShoreMask"
{
    Properties
    {
        _MainTex("Height Map", 2D) = "white" {}
        _SeaLevel("Sea Level", float) = 0.0
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
                float _SeaLevel;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                float getSeaDistance(float2 coord, float steps, float stepSize)
                {
                    float dist = 0;
                    float stepsTotal = steps * steps;
                    float currentHeight = tex2D(_MainTex, coord).r;
                    if (currentHeight <= _SeaLevel)
                        return 1;
                    currentHeight -= _SeaLevel;
                    float currentStepSize = max(0,stepSize - (currentHeight * 0.2));
                    for (float i = -steps; i <= steps; i += 1)
                    {
                        for (float n = -steps; n <= steps; n += 1)
                        {
                            float level = tex2D(_MainTex, coord + float2(i*currentStepSize,n*currentStepSize)).r;
                            if (level <= _SeaLevel)
                                dist += 1 / stepsTotal;
                        }
                    }
                    return dist;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return getSeaDistance(i.uv, 4, 0.004);
                }
                ENDCG
            }
        }
}
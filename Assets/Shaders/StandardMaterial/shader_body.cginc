// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

sampler2D _MainTex;
sampler2D _BumpMap;
uniform float4 _BumpMap_TexelSize;
float _SeaLevel;
float _AlphaMultiplier;

float4 _DiffuseCoefficient;


struct Input
{
    float2 uv_MainTex;
    float3 worldPos;
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

void surf (Input IN, inout SurfaceOutput o)
{
    fixed4 c = _DiffuseCoefficient;
    c *= tex2D (_MainTex, IN.uv_MainTex);

    c.a *= _AlphaMultiplier;

    float seaAmount = pow(max(0, -(IN.worldPos.y - _SeaLevel)) * 0.02, 0.4);
    seaAmount = min(1, seaAmount);
    fixed4 seaColor = fixed4(0, 0, 0, 1);
    c.rgb = lerp(c, seaColor, seaAmount).rgb;

    float3 bumpNormal = normalFromBumpMap(IN.uv_MainTex, 5);

    o.Normal = bumpNormal;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
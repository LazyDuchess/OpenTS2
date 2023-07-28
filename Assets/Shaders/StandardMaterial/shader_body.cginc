// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

sampler2D _MainTex;
float _SeaLevel;
float _AlphaMultiplier;

struct Input
{
    float2 uv_MainTex;
    float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

    c.a *= _AlphaMultiplier;

    float seaAmount = pow(max(0, -(IN.worldPos.y - _SeaLevel)) * 0.02, 0.4);
    seaAmount = min(1, seaAmount);
    fixed4 seaColor = fixed4(0, 0, 0, 1);
    c.rgb = lerp(c, seaColor, seaAmount).rgb;

    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
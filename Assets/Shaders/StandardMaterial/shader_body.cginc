// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

sampler2D _MainTex;
float _AlphaMultiplier;

struct Input
{
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

    c.a *= _AlphaMultiplier;

    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
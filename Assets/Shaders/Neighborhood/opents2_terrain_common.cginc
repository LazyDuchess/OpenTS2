sampler2D _MainTex;
sampler2D _Shore;
sampler2D _Variation1;
sampler2D _Variation2;
sampler2D _MatCap;
sampler2D _CliffTex;
sampler2D _ShoreMask;
sampler2D _Roughness;
sampler2D _Roughness1;
sampler2D _Roughness2;
float4 _MainTex_ST;
float4 _LightVector;
float _Subtract;
float _Ambient;
float _SeaLevel;

#define TERRAIN_APPDATA \
float4 vertex : POSITION; \
float2 uv : TEXCOORD0; \
float3 normal : NORMAL; \
float4 color : COLOR

#define TERRAIN_V2F \
float2 uv : TEXCOORD0; \
float2 matcapUv : TEXCOORD1; \
UNITY_FOG_COORDS(1) \
float4 vertex : SV_POSITION; \
float4 color : COLOR; \
float cliff : TEXCOORD2; \
float2 shadowUv : TEXCOORD3; \
float height : TEXCOORD4; \
float roughness : TEXCOORD5

#define TERRAIN_VERT \
v2f o; \
o.vertex = UnityObjectToClipPos(v.vertex); \
o.uv = TRANSFORM_TEX(v.uv, _MainTex); \
UNITY_TRANSFER_FOG(o, o.vertex); \
float3 lightingDirection = -normalize(_LightVector.xyz); \
float3 lightCross = cross(lightingDirection, float3(0.0, 1.0, 0.0)); \
float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz; \
float lightDot = dot(worldNormal, lightingDirection); \
float lightDotRight = dot(worldNormal, lightCross); \
o.matcapUv = max(0, ((float2(lightDot, lightDot) + float2(lightDotRight, 0.0))) - _Subtract); \
o.matcapUv += float2(_Ambient, _Ambient); \
o.uv = float2(v.vertex.x, v.vertex.z) * float2(_MainTex_ST.x, _MainTex_ST.y); \
o.uv += float2(_MainTex_ST.x, _MainTex_ST.y); \
o.color = v.color; \
o.color.b = 0; \
o.roughness = v.color.b; \
o.cliff = max(0, min(1, pow(-(dot(worldNormal, float3(0.0, 1.0, 0.0)) - 1), 2) * 3)); \
o.shadowUv = getNeighborhoodUV(v.vertex); \
o.height = v.vertex.y; \
if (v.vertex.y <= _SeaLevel) \
o.color.b = 1
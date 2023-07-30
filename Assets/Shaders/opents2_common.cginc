float _TerrainWidth;
float _TerrainHeight;
sampler2D _ShadowMap;

float2 getNeighborhoodUV(float3 worldPos) {
	return float2(worldPos.x / (_TerrainWidth * 10), worldPos.z / (_TerrainHeight * 10));
}

float getNeighborhoodShade(float3 worldPos) {
	return tex2D(_ShadowMap, getNeighborhoodUV(worldPos));
}
#define PULL_UP(amount) \
float3 viewHeading = normalize(WorldSpaceViewDir(v.vertex)); \
float viewUpDot = abs(dot(viewHeading, float3(0, 1, 0))); \
o.vertex.z += sqrt(amount * viewUpDot);
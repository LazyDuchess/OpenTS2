Shader "OpenTS2/StandardMaterial/Opaque"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _AlphaMultiplier ("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows

        #include "Assets/Shaders/StandardMaterial/shader_body.cginc"

        ENDCG
    }
    FallBack "Diffuse"
}

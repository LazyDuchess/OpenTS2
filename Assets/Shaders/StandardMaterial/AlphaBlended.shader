Shader "OpenTS2/StandardMaterial/AlphaBlended"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _AlphaMultiplier("Alpha Multiplier", Range(0.0, 1.0)) = 1.0

        _SeaLevel("Sea Level", float) = -100.0
    }
    SubShader
    {
        ZWrite Off
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows alpha:blend

        #include "Assets/Shaders/StandardMaterial/shader_body.cginc"

        ENDCG
    }
    FallBack "Diffuse"
}

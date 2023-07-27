Shader "OpenTS2/StandardMaterial/AlphaCutOut"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _AlphaMultiplier ("Alpha Multiplier", Range(0.0, 1.0)) = 1.0

        _AlphaCutOff ("AlphaCutOff", Float) = 1.0

        _SeaLevel("Sea Level", float) = -100.0
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "Queue"="AlphaTest" }

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows alphatest:_AlphaCutOff

        #include "Assets/Shaders/StandardMaterial/shader_body.cginc"

        ENDCG
    }
    FallBack "Diffuse"
}

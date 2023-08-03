Shader "OpenTS2/StandardMaterial/Opaque"
{
    Properties
    {
        // Parameters common to all StandardMaterial shaders.
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _AlphaMultiplier ("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
        _BumpMap("Bump Map", 2D) = "white" {}
        _DiffuseCoefficient("Diffuse color coefficient", Color) = (1, 1, 1, 1)

        _SeaLevel("Sea Level", float) = -100.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Stencil {
                Ref 0
                Comp Always
                Pass Replace
            }
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows

        #include "Assets/Shaders/StandardMaterial/shader_body.cginc"

        ENDCG
    }
    FallBack "Diffuse"
}

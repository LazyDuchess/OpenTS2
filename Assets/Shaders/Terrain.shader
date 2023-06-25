Shader "OpenTS2/Terrain" {
    SubShader{
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf SimpleLambert

        struct Output
      {
          fixed3 Albedo;  // diffuse color
          fixed3 Normal;  // tangent space normal, if written
          fixed3 Emission;
          half Specular;  // specular power in 0..1 range
          fixed Gloss;    // specular intensity
          fixed Alpha;    // alpha for transparencies
          fixed Shadow;
      };

        half4 LightingSimpleLambert(Output s, half3 lightDir, half atten) {
              half NdotL = max(0,dot(s.Normal, lightDir));
              half4 c;
              atten -= s.Shadow;
              atten = max(atten, 0.0);
              float3 shading = _LightColor0.rgb * (NdotL * atten);
              c.rgb = s.Albedo * shading;
              
              //c.rgb = NdotL;
              c.a = s.Alpha;
              return c;
          }

      struct Input {
          float4 color : COLOR;
      };

      

      void surf(Input IN, inout Output o) {
          o.Albedo = 1;
          o.Shadow = IN.color.r;
      }
      ENDCG
    }
        Fallback "Diffuse"
}
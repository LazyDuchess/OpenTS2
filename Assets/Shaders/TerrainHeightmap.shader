Shader "OpenTS2/TerrainHeightmap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        // make fog work
        #pragma multi_compile_fog
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
            float height : TEXCOORD1;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vert(appdata v)
        {
            v2f o;
            o.height = v.vertex.y / 1000;
            o.vertex = float4(v.vertex.x - 128 * 5, -(v.vertex.z - 128 * 5) + 1, 0, 128 * 5);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            // sample the texture
            fixed4 col = float4(i.height,i.height,i.height,1);
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col;
    }
    ENDCG
}
    }
}
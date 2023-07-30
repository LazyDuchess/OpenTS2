Shader "OpenTS2/TerrainHeightmap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HeightDivision("Height Division", float) = 1000.0
        _Width("Terrain Width", float) = 128.0
        _Height("Terrain Height", float) = 128.0
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
        float _HeightDivision;
        float _Width;
        float _Height;

        v2f vert(appdata v)
        {
            v2f o;
            o.height = v.vertex.y / _HeightDivision;
            float xPos = (v.vertex.x / _Width * 0.2) - 1;
            float yPos = -((v.vertex.z / _Height * 0.2) - 1);
            float wPos = 1.0;
            o.vertex = float4(xPos, yPos, 0, wPos);
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
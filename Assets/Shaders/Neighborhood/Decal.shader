// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "OpenTS2/NeighborhoodDecal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Location ("Location", Vector) = (0,0,0,0)
        _Rotation("Rotation", Vector) = (0,0,0,0)
        _Size("Size", float) = 80.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-2"  }
        LOD 100

        Pass
        {
            ZWrite Off
            Offset -1, -1
            Blend SrcAlpha OneMinusSrcAlpha
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
                float4 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Size;
            float4 _Location;
            float4 _Rotation;

            float2 rotateUvs(float2 uv, float rotation)
            {
                float sinX = sin(rotation);
                float cosX = cos(rotation);
                float sinY = sin(rotation);
                float2x2 rotationMatrix = float2x2(cosX, -sinX, sinY, cosX);
                return mul(uv, rotationMatrix);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 minPos = float4(_Location.x, _Location.y, _Location.z, _Location.w);
                minPos -= float4(_Size / 2, _Size / 2, _Size / 2, _Size / 2);
                float4 maxPos = minPos + float4(_Size, _Size, _Size, _Size);
                o.worldPos -= minPos;
                o.worldPos /= _Size;
                o.uv = rotateUvs(float2(o.worldPos.x-0.5, o.worldPos.z-0.5), radians(-_Rotation.y));
                o.uv += float2(0.5, 0.5);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
                if (i.uv.x < 0 || i.uv.y < 0 || i.uv.x > 1 || i.uv.y > 1)
                clip(-1);
            /*
                if (i.worldPos.x < 0 || i.worldPos.x > 1 || i.worldPos.z < 0 || i.worldPos.z > 1)
                    clip(-1);*/
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

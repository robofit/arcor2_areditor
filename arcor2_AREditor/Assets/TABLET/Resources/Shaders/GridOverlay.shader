// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/GridOverlay"
{
    Properties
    {
        _GridSize("Grid Size", Float) = 10
        _Grid2Size("Grid 2 Size", Float) = 160
        _Grid3Size("Grid 3 Size", Float) = 320
        _Alpha("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
                
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _GridSize;
            float _Grid2Size;
            float _Grid3Size;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_ObjectToWorld, v.vertex).xz;
                return o;
            }

            float DrawGrid(float2 uv, float sz, float aa)
            {
                float aaThresh = aa;
                float aaMin = aa * 0.1;

                float2 gUV = uv / sz + aaThresh;

                float2 fl = floor(gUV);
                gUV = frac(gUV);
                gUV -= aaThresh;
                gUV = smoothstep(aaThresh, aaMin, abs(gUV));
                float d = max(gUV.x, gUV.y);

                return d;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed r = DrawGrid(i.uv, _GridSize, 0.03) * 0.8;
                fixed b = DrawGrid(i.uv, _Grid2Size, 0.005) * 0.8;
                fixed g = DrawGrid(i.uv, _Grid3Size, 0.002) * 0.8;
                return float4(r * _Alpha, g * _Alpha, b * _Alpha, (r + b + g) * _Alpha);
            }
            ENDCG
        }
    }
}

//Clipping shader based on RonjaTutorials: https://www.ronja-tutorials.com/post/021-plane-clipping/

Shader "ClippingColorChange" {
    Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR]_Emission ("Emission", color) = (0,0,0)

        _AlternateColor ("Alternate Color", Color) = (0.6, 0.29, 0.29, 0)
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Smoothness;
        half _Metallic;
        half3 _Emission;
        float4 _Plane;
        float4 _AlternateColor;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf (Input i, inout SurfaceOutputStandard o) {
            float distance = dot(i.worldPos, _Plane.xyz);
            distance = distance + _Plane.w;

            if (distance > 0) {
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col *= _Color;
                o.Albedo = col.rgb;
            } else {
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col *= _Color;
                o.Albedo = col.rgb + _AlternateColor.rgb;
            }
        }
        ENDCG
    }
    FallBack "Standard"
}

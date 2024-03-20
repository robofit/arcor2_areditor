Shader "ClippingColorChange" {
    Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR]_Emission ("Emission", color) = (0,0,0)

        _AlternateColor ("Alternate Color", Color) = (1, 0, 0, 0) // New property for the alternate color
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
        float4 _AlternateColor; // Declare variable for the alternate color

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf (Input i, inout SurfaceOutputStandard o) {
            float distance = dot(i.worldPos, _Plane.xyz);
            distance = distance + _Plane.w;

            // Instead of clipping, we use a conditional to render in different colors
            if (distance > 0) {
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col *= _Color;
                o.Albedo = col.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Smoothness;
                o.Emission = _Emission;
            } else {
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col *= _Color;
                o.Albedo = col.rgb + _AlternateColor.rgb; // Render in alternate color if below the clipping plane
                o.Metallic = 0;
                o.Smoothness = 0;
                o.Emission = _Emission;
            }
        }
        ENDCG
    }
    FallBack "Standard"
}

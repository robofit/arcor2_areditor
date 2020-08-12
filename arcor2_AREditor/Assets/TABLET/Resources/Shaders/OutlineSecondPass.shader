Shader "Outline/SecondPass" {

    Properties{
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0, .5)) = .1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    float _OutlineWidth;
    float4 _OutlineColor;
    ENDCG

    Subshader
    {
        Zwrite Off
        ZTest Always
        Tags {
            "Queue" = "Transparent+10"
        }

        // Render outline
        Pass {
            // Set stencil mask
            Stencil {
                Ref 2
                Comp Equal
            }
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert( float4 vertex : POSITION ) : SV_Position {
                float4x4 m = unity_ObjectToWorld;
                float3 scale = float3(
                    length( float3( m[ 0 ][ 0 ], m[ 0 ][ 1 ], m[ 0 ][ 2 ] ) ),
                    length( float3( m[ 1 ][ 0 ], m[ 1 ][ 1 ], m[ 1 ][ 2 ] ) ),
                    length( float3( m[ 2 ][ 0 ], m[ 2 ][ 1 ], m[ 2 ][ 2 ] ) )
                );
                vertex.xyz += vertex.xyz * _OutlineWidth * 1.0f / scale;
                return UnityObjectToClipPos( vertex );
            }

            half4 frag() : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}

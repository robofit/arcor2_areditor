Shader "Outline/ModelFirstPass" {

    Properties{
      //_OutlineWidth("Outline Width", Range(0, .5)) = .1
      _OutlineWidth("Outline Width", Float) = .1
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    float _OutlineWidth;
    ENDCG

    Subshader
    {
        Zwrite Off
        //ColorMask 0
        ZTest Always
        Tags {
            "Queue" = "Geometry-1"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        // Render outer mask
        Pass {
            // Set stencil mask
            Stencil {
                Ref 2
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //float4 vert( float4 vertex : POSITION ) : SV_Position {
            //    vertex.xyz += vertex.xyz * _OutlineWidth;
            //    return UnityObjectToClipPos( vertex );
            //}

            struct v2f {
                    float4 pos : SV_POSITION;
                };

            v2f vert(appdata_base v) {
                v2f o;
                v.vertex.xyz += normalize(v.vertex.xyz) * _OutlineWidth;
                //v.vertex.xyz += v.normal * _OutlineWidth;
                v.normal *= -1;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag() : SV_Target {
                return float4( 0.0f, 0.0f, 0.0f, 0.0f );
            }
            ENDCG
        }

        // Render inner mask
        Pass {
            ColorMask 0
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert( float4 vertex : POSITION ) : SV_Position {
                return UnityObjectToClipPos(vertex);
            }

            half4 frag() : SV_Target {
                return float4( 0.0f, 0.0f, 0.0f, 0.0f );
            }
            ENDCG
        }
    }
}

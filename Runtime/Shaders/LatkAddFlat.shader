// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Latk/AddFlat" {

	Properties {
		[PerRendererData]_Color ("Color", Color) = (0.5,0.5,0.5,0.5)
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		ColorMask RGB
		Cull Off Lighting Off //ZWrite Off
	
		SubShader {
			Pass {
		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				fixed4 _Color;
			
				struct appdata_t {
					float4 vertex : POSITION;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
				};
			
				v2f vert (appdata_t v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}
		
				fixed4 frag (v2f i) : SV_Target {			
					fixed4 col = _Color;
					return col;
				}
				ENDCG 
			}
		}	
	}

}

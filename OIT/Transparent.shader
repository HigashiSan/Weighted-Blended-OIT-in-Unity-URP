Shader "cdc/TransparentOBJShader"
{
    Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_Alpha("Alpha", Range(0.0, 1.0)) = 0.5
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	

				Pass {
			Tags { "LightMode" = "DoOIT" }

			ZWrite Off
			Blend 0 One One
			Blend 1 Zero OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Alpha;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD2;
				float z : TEXCOORD3;
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.z = abs(UnityObjectToViewPos(v.vertex).z);

				return o;
			}

			float w(float z, float alpha) {
					return alpha * max(1e-2, min(3 * 1e3, 0.03 / (1e-5 + pow(z / 200, 4))));
			}

			void frag(v2f i, out float4 color : SV_Target0, out float4 alpha : SV_Target1){

				fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo.rgb;
			
				fixed3 C = (albedo.rgb) * _Alpha;

				color = float4(C, _Alpha) * w(i.z, _Alpha);
				alpha = albedo.aaaa;
			}

			ENDCG
		}
	}
}

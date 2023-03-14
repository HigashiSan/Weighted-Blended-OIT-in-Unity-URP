Shader "cdc/Transparent"
{
    Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
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
			sampler2D _BumpMap;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 lightDir: TEXCOORD0;
				float3 viewDir : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float z : TEXCOORD3;
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				TANGENT_SPACE_ROTATION;
				// Transform the light direction from object space to tangent space
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
				// Transform the view direction from object space to tangent space
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

				// Camera-space depth
				o.z = abs(UnityObjectToViewPos(v.vertex).z);
				//o.z = abs(mul(UNITY_MATRIX_MV, v.vertex).z);

				return o;
			}

			float w(float z, float alpha) {
					return alpha * max(1e-2, min(3 * 1e3, 0.03 / (1e-5 + pow(z / 200, 4))));
			}

			void frag(v2f i, out float4 color : SV_Target0, out float4 alpha : SV_Target1){
				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir = normalize(i.viewDir);
				fixed3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));

				fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo.rgb;
				fixed3 diffuse = _LightColor0.rgb * albedo.rgb * max(0, dot(tangentNormal, tangentLightDir));
			
				float3 C = (ambient + diffuse) * albedo.a;

				color = float4(C, albedo.a) * w(i.z, albedo.a);
				alpha = albedo.aaaa;
			}

			ENDCG
		}
	}
}

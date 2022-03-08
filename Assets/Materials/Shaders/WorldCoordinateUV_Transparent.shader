Shader "AR/World Coordinate UV, Transparent"
{
	Properties
	{
		_MainTex       ("Texture", 2D)                         = "white" {}
		_Color         ("Tint",    Color)                      = (1, 1, 1, 1)
		_BlendThreshold("Axis Blend Threshold", Range(0, 0.5)) = 0
		_BlendFactor   ("Axis Blend Factor", Range(1, 20))     = 2
		_SurfaceOffset ("Surface Offset", Range(0, 0.1))       = 0.05
	}

	SubShader
	{
		Tags { "Render Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex      : SV_POSITION;
				float4 worldPos    : POSITION1;
				float4 worldNormal : NORMAL;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float2    _MainTex_ST;
			float4    _Color;
			float     _BlendThreshold;
			float     _BlendFactor;
			float     _SurfaceOffset;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex      = UnityObjectToClipPos(v.vertex + _SurfaceOffset * v.normal);
				o.worldPos    = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
				o.worldNormal = mul(UNITY_MATRIX_M, float4(v.normal.xyz, 0));
				
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				// triplanar blending
				// https://forum.unity.com/threads/mapping-texture-to-world-position-instead-of-object-position.94766/
				//
				float3 uvs = i.worldPos.xyz;
				float3 blending = saturate(abs(i.worldNormal.xyz) - _BlendThreshold) + 0.02;
				blending  = pow(blending, _BlendFactor);
				blending /= dot(blending, float3(1.0, 1.0, 1.0));
				float4 col = blending.x * tex2D(_MainTex, uvs.yz * _MainTex_ST);
				       col = blending.y * tex2D(_MainTex, uvs.xz * _MainTex_ST) + col;
				       col = blending.z * tex2D(_MainTex, uvs.xy * _MainTex_ST) + col;

				col *= _Color;

				return col;
			}

			ENDCG
		}
	}
}

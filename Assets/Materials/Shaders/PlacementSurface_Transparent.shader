Shader "AR/World Coordinate UV Plane"
{
	Properties
	{
		_MainTex("Texture",      2D)    = "white" {}
		_Color  ("Tint",         Color) = (1,1,1,1)
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
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4    _MainTex_ST;
			float4    _Color;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex  = UnityObjectToClipPos(v.vertex);
				float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv      = TRANSFORM_TEX(wp.xz, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				fixed4 col = tex2D(_MainTex, fmod(i.uv, 1));
				col *= _Color;

				return col;
			}

			ENDCG
		}
	}
}

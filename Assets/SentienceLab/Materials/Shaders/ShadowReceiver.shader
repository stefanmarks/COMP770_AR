Shader "FX/Shadow Receiver"
{
	Properties
	{
		_ShadowColor("Shadow Color", COLOR) = (0,0,0,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			float4 _ShadowColor;

			struct appdata {
				float4 vertex    : POSITION;
				float4 texcoord  : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos        : SV_POSITION;
				LIGHTING_COORDS(0, 1)
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed  attenuation = 1 - LIGHT_ATTENUATION(i);
				fixed4 finalColor  = fixed4(_ShadowColor.rgb, _ShadowColor.a * attenuation);
				return finalColor;
			}
			ENDCG
		}

	}

	Fallback "VertexLit"
}

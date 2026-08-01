// Ghost material for the local player's own character while aiming. The character's own shaders
// (Custom/ColorSwap, Toon/Lit) are opaque with no blend mode, so there is nothing on them to turn
// down - CharacterFader swaps this in instead and animates its alpha.
//
// Lives in Resources so it is always included in a build; a shader only reachable through
// Shader.Find would get stripped.
Shader "Custom/CharacterFade"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				return col;
			}
			ENDCG
		}
	}

	Fallback "Unlit/Transparent"
}

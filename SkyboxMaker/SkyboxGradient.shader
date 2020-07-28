// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SkyboxGradient"
{
	Properties
	{
		_LightEdge("LightEdge", Color) = (1,1,1,1)
		_Light("Light", Color) = (1,1,1,1)
		_Dark("Dark", Color) = (1,1,1,1)
		_DarkEdge("Dark Edge", Color) = (1,1,1,1)

		_LightStrength("Light Strength", float) = 1
		_DarkStrength("Dark Strength", float) = 1

		_GradientSize("Gradient Size", float) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos =mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 _Dark;
			fixed4 _Light;
			fixed4 _LightEdge;
			fixed4 _DarkEdge;

			float _LightStrength;
			float _DarkStrength;
			float _GradientSize;

			fixed4 frag(v2f i) : SV_Target
			{
				float wp = saturate(( i.worldPos.y - -_GradientSize) / (_GradientSize - -_GradientSize));

				fixed4 col;
				col = lerp(_Dark, _Light, wp);

				float darkpow = pow((1 - wp), _DarkStrength);
				float lightpow = pow(wp , _LightStrength);

				col.xyz = lerp(col, _DarkEdge, darkpow);
				col.xyz = lerp(col, _LightEdge, lightpow);

				col.a = 1;

				return col;
			}
			ENDCG
		}
	}
}


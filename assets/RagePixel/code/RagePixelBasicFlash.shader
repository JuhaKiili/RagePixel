Shader "RagePixel/Basic (Flash)" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TexWidth ("Texture width", Float) = 128.0
		_TexHeight ("Texture height", Float) = 128.0

	}
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Pass {
			ZWrite Off
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float _TexWidth;
				float _TexHeight;

				struct VertOut
				{
					float4 position : POSITION;
					float4 color : COLOR;
					float4 texcoord : TEXCOORD0;
				};

				struct VertIn
				{
					float4 vertex : POSITION;
					float4 color : COLOR;
					float4 texcoord : TEXCOORD0;
				};

				VertOut vert(VertIn input)
				{
					VertOut output;
					output.position = mul(UNITY_MATRIX_MVP,input.vertex);
					output.color = input.color;
					output.texcoord = float4( input.texcoord.xy, 0, 0);
					return output;
				}

				struct FragOut
				{
					float4 color : COLOR;
				};
								
				FragOut frag(VertIn input)
				{
					FragOut output;
					half4 texcol = tex2D(_MainTex, float2(float(int(input.texcoord.x * _TexWidth) + 0.5) / _TexWidth, float(int(input.texcoord.y * _TexHeight) + 0.5) / _TexHeight));
					//output.color = half4(input.color.b * texcol.r, input.color.g * texcol.g, input.color.r * texcol.b, input.color.a * texcol.a);
					output.color = texcol * input.color;
					return output;
				}
            ENDCG
		}

	} 
	FallBack "Diffuse"
}

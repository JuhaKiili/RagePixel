Shader "RagePixel/Basic" {
	Properties {
		_MainTex ("Texture1 (RGB)", 2D) = "white" {}
	}

	Category {
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		Lighting Off
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "Texcoord", Texcoord
		}
		
		SubShader {
			Pass {
				ZWrite Off
				Cull off
				Blend SrcAlpha OneMinusSrcAlpha
				SetTexture [_MainTex] {
					Combine texture * primary, texture * primary
				}
			}
		}
	}
}
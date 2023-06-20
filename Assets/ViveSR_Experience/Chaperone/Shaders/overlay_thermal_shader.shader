Shader "ViveSR/Overlay/Chaperone_Thermal" {

	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_AlphaTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Shade("Shade", Range(1,10)) = 2.5
		_Alpha("Alpha", Float) = 0.4
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		Pass{
		CGPROGRAM
#include "UnityCG.cginc"
#pragma vertex vert_img
#pragma fragment frag
		sampler2D _MainTex;
		sampler2D _AlphaTex;
		float _Shade;
		float _Alpha;

	float4 thermal(sampler2D tex, float2 uv)
	{
		float3 tc = float3(1.0, 0.0, 0.0);
		fixed4 col = tex2D(tex, uv);
		float3 colors[3];
		colors[0] = float3(0.0, 0.0, 1.0);
		colors[1] = float3(1.0, 1.0, 0.0);
		colors[2] = float3(1.0, 0.0, 0.0);
		float lum = (col.r + col.g + col.b) / _Shade;
		int ix = (lum < 0.5) ? 0 : 1;
		tc = lerp(colors[ix], colors[ix + 1], (lum - float(ix)*0.5) / 0.5);

		return fixed4(tc, 1.0);
	}
	
	float4 frag(v2f_img IN) : COLOR{
		float a = tex2D(_AlphaTex, float2(IN.uv.x, 1 - IN.uv.y)).a;
		a = a > 0 ? _Alpha : 0;

	fixed4 main = thermal(_MainTex, float2(IN.uv.x, 1 - IN.uv.y));
	return float4(main.r, main.g, main.b, a);

	}

		ENDCG
	}
	}
		FallBack "Diffuse"
}
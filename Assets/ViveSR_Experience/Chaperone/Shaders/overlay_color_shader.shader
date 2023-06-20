Shader "ViveSR/Overlay/Chaperone_Color" {

	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_AlphaTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_Color("Main Color", Color) = (1.0,1.0,1.0,1.0)
		_Threshold("Threshold", Range(0.01,0.99)) = 0.5
		_DeltaX("Delta X", Float) = 0.001
		_DeltaY("Delta Y", Float) = 0.001
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
	float4 _Color;
	float _Threshold;
	float _DeltaX;
	float _DeltaY;
	float _Alpha;

	float sobel(sampler2D tex, float2 uv) {
		float2 delta = float2(_DeltaX, _DeltaY);

		float4 hr = float4(0, 0, 0, 0);
		float4 vt = float4(0, 0, 0, 0);

		hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
		hr += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * -1.0;
		hr += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
		hr += tex2D(tex, (uv + float2(0.0,  0.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0,  0.0) * delta)) * -2.0;
		hr += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
		hr += tex2D(tex, (uv + float2(0.0,  1.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0,  1.0) * delta)) * -1.0;

		vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) *  2.0;
		vt += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(0.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
		vt += tex2D(tex, (uv + float2(0.0,  1.0) * delta)) * -2.0;
		vt += tex2D(tex, (uv + float2(1.0,  1.0) * delta)) * -1.0;

		//return hr;
		return sqrt(hr * hr + vt * vt);
	}

	float4 frag(v2f_img IN) : COLOR{
		float a = tex2D(_AlphaTex, float2(IN.uv.x, 1 - IN.uv.y)).a;
	a = a > 0 ? _Alpha : 0;

	fixed4 main = tex2D(_MainTex, float2(IN.uv.x, 1 - IN.uv.y));
	return float4(main.r, main.g, main.b, a);

	//float s = sobel(_MainTex, float2(IN.uv.x, 1 - IN.uv.y));
	//fixed4 col = s < _Threshold ? main : main * s * 2;
	//return float4(col.r, col.g, col.b, a);
	}

		ENDCG
	}
	}
		FallBack "Diffuse"
}
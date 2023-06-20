Shader "ViveSR/Overlay/Chaperone_Depth"
{
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_AlphaTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_DepthTex("Depth (Float)", 2D) = "white" {}
		_ColorMap("Colormap", 2D) = "white" {}
		_Threshold("Threshold", Range(0.01,0.99)) = 0.5
		_DeltaX("Delta X", Float) = 0.001
		_DeltaY("Delta Y", Float) = 0.001
		_MaxDepth("Max Depth", Float) = 250
		_MinDepth("Min Depth", Float) = 30
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
	sampler2D _DepthTex;
	sampler2D _ColorMap;
	float _Threshold;
	float _DeltaX;
	float _DeltaY;
	float _MaxDepth;
	float _MinDepth;
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

		fixed3 _Color;
		fixed3 depth = tex2D(_DepthTex, float2(IN.uv.x, 1 - IN.uv.y));
		if (depth.x > 0)
		{
			if (depth.x > _MaxDepth) depth.x = _MaxDepth;
			else if (depth.x < _MinDepth) depth.x = _MinDepth;

			fixed2 new_uv = fixed2(0.5, (depth.x - _MinDepth) / (_MaxDepth - _MinDepth));
			if (new_uv.y < 0.05) new_uv.y = 0.05;
			if (new_uv.y > 0.95) new_uv.y = 0.95;
			_Color = tex2D(_ColorMap, new_uv).rgb;
		}
		else
			_Color = float3(0, 0, 0);

		float s = sobel(_MainTex, float2(IN.uv.x, 1 - IN.uv.y));
		fixed3 col = _Color * s;
		// Alpha depends on the result from human dection
		float a = tex2D(_AlphaTex, float2(IN.uv.x, 1 - IN.uv.y)).a;
		a = col < _Threshold && depth.x > 0 && a > 0 ? _Alpha : 0;
		return float4(col, a);
	}

		ENDCG
	}
	}
		FallBack "Diffuse"
}
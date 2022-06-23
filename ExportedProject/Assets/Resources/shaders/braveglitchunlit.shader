Shader "Brave/Internal/GlitchUnlit" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_GlitchInterval ("Glitch interval time [seconds]", Float) = 0.1
		_DispProbability ("Displacement Glitch Probability", Float) = 0.4
		_DispIntensity ("Displacement Glitch Intensity", Float) = 0.01
		_ColorProbability ("Color Glitch Probability", Float) = 0.4
		_ColorIntensity ("Color Glitch Intensity", Float) = 0.04
		[MaterialToggle] _WrapDispCoords ("Wrap disp glitch (off = clamp)", Float) = 1
		[MaterialToggle] _DispGlitchOn ("Displacement Glitch On", Float) = 1
		[MaterialToggle] _ColorGlitchOn ("Color Glitch On", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "tk2d/CutoutVertexColor"
}
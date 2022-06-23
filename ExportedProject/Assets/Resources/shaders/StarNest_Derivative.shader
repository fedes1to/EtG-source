Shader "Brave/Internal/StarNest_Derivative" {
	Properties {
		_Iterations ("Iterations", Float) = 17
		_FormUParam ("Form U Param", Float) = 0.53
		_VolSteps ("Vol Steps", Float) = 20
		_StepSize ("Step Size", Float) = 0.1
		_Zoom ("Zoom", Float) = 0.8
		_Tile ("Tile", Float) = 0.85
		_Speed ("Speed", Float) = 0.01
		_PositionX ("Position X", Float) = 0
		_PositionY ("Position Y", Float) = 0
		_Brightness ("Brightness", Float) = 0.0015
		_DistFading ("Dist Fading", Float) = 0.73
		_Saturation ("Saturation", Float) = 0.85
		_Gain ("Gain", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}
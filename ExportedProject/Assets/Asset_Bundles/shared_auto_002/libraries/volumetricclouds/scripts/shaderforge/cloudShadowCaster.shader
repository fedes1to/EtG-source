Shader "Cloud/ShaderForge/ShadowCaster" {
	Properties {
		_PerlinNormalMap ("PerlinNormalMap", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 3000
		_Density ("Density", Float) = -0.25
		_Alpha ("Alpha", Float) = 5
		_AlphaCut ("AlphaCut", Float) = 0.01
		_Speed ("Speed", Float) = 0.1
		_SpeedSecondLayer ("SpeedSecondLayer", Float) = 4
		_WindDirection ("WindDirection", Vector) = (1,0,0,0)
		_MipLevel ("MipLevel", Float) = 0
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
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
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}
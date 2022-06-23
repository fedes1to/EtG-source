Shader "Cloud/ShaderForge/Standard" {
	Properties {
		_BaseColor ("BaseColor", Vector) = (1,1,1,0.5)
		_Shading ("Shading", Vector) = (0,0,0,0.5)
		_DepthIntensity ("DepthIntensity", Float) = 0.5
		_PerlinNormalMap ("PerlinNormalMap", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 3000
		_Density ("Density", Float) = -0.25
		_Alpha ("Alpha", Float) = 5
		_AlphaCut ("AlphaCut", Float) = 0.01
		_Speed ("Speed", Float) = 0.1
		_SpeedSecondLayer ("SpeedSecondLayer", Float) = 4
		_WindDirection ("WindDirection", Vector) = (1,0,0,0)
		_CloudNormalsDirection ("CloudNormalsDirection", Vector) = (1,1,-1,0)
		_MipLevel ("Mip Level", Float) = 0
		_EdgeBlend ("EdgeBlend", Range(0, 10)) = 2
		_ZOffset ("Z Offset", Float) = 0
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
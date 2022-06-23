Shader "Brave/Internal/GBuffer_LightRenderer_Fast" {
	Properties {
		_LightPos ("Light Position", Vector) = (0,0,0,0)
		_LightRadius ("Light Radius", Float) = 1
		_LightIntensity ("Light Intensity", Float) = 0
		_LightColor ("Light Color", Vector) = (0,0,0,0)
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
}
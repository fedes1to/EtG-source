Shader "Brave/Internal/RadiusIndicatorEffect" {
	Properties {
		_Radius ("Radius", Float) = 5
		_PxWidth ("Pixel Width", Float) = 1
		_RingColor ("Ring Color", Vector) = (1,1,1,1)
		_WorldCenter ("World Center", Vector) = (0,0,0,0)
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
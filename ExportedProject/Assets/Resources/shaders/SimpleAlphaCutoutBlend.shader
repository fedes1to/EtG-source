Shader "Unlit/SimpleAlphaCutoutBlend" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Threshold ("Threshold", Range(0, 1.25)) = 1
		_BaseAlphaMax ("Alpha", Range(0, 1)) = 0.9
		_AddlTex ("Additional Texture", 2D) = "black" {}
		_UseAddlTex ("Use Additional Texture", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}
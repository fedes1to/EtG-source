Shader "Brave/GoopShader" {
	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
		_WorldTex ("World Tex", 2D) = "white" {}
		_TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,1)
		_OpaquenessMultiply ("Opaqueness", Float) = 0.5
		_BrightnessMultiply ("Brightness", Float) = 1
		_OilGoop ("Oily", Float) = 0
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
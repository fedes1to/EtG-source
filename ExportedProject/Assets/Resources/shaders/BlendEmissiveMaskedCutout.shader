Shader "Brave/BlendEmissiveMaskedCutout" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaskTex ("Mask", 2D) = "white" {}
		_OverrideColor ("Override Color", Vector) = (1,1,1,0)
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_RevealAmount ("RevealAmount", Range(0, 1)) = 1
		_BrightnessWarble ("Warble", Float) = 1
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
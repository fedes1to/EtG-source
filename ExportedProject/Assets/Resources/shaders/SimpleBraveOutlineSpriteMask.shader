Shader "Brave/Internal/SimpleBraveOutlineSpriteMask" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_SelfIllumination ("Min Brightness", Float) = 0
		_LuminanceCutoff ("Luminance Cutoff", Float) = 0
		_AtlasData ("Atlas Data", Vector) = (1,1,1,1)
		_ValueMaximum ("Max Value", Float) = 1
		_ValueMinimum ("Min value", Float) = 1
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
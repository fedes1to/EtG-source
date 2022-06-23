Shader "tk2d/BlendVertexColorFadeRange" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Saturation ("Saturation", Float) = 1
		_OverrideColor ("Override Color", Vector) = (1,1,1,0)
		_XFadeStart ("X Fade Start", Float) = 0
		_XFadeEnd ("X Fade End", Float) = 1
		_YFadeStart ("Y Fade Start", Float) = 1
		_YFadeEnd ("Y Fade End", Float) = 0
		_DivPower ("Div Power", Float) = 1
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
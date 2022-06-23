Shader "Brave/PlayerShader" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_OverrideColor ("Override Color", Vector) = (1,1,1,0)
		_FlatColor ("Flat Color", Vector) = (0.2,0.3,1,0)
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_SpecialFlags ("Special Flags", Vector) = (0,0,0,0)
		_StencilVal ("Stancil", Float) = 146
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
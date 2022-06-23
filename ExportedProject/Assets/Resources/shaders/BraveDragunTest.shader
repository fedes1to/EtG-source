Shader "Brave/Internal/DragunTest" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CracksTex ("Cracks", 2D) = "black" {}
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_CrackBaseColor ("Crack Base Color", Vector) = (1,0.1,0,1)
		_CharAmount ("Char Amount", Range(0, 1)) = 0
		_CrackAmount ("Crack Amount", Float) = 0
		_CrackShadow ("Crack Shadow", Float) = 10
		_RectangleAmount ("Rectangle", Range(0, 1)) = 0
		_CircleAmount ("Circle", Range(0, 1)) = 0
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
	Fallback "tk2d/CutoutVertexColor"
}
Shader "Brave/LitBlendUber" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_PaletteTex ("Palette (RGB)", 2D) = "white" {}
		[Toggle] _Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_EmissivePower ("Emissive Power", Float) = 0
		_EmissiveColorPower ("Emissive Color Power", Float) = 7
		_EmissiveColor ("Emissive Color", Vector) = (1,1,1,1)
		_OverrideColor ("Tint Color", Vector) = (1,1,1,0)
		_ValueMaximum ("Max Value", Float) = 1
		_ValueMinimum ("Min value", Float) = 0.5
		_BurnAmount ("Burn", Range(0, 1)) = 0
		_RectangleAmount ("Rectangle", Range(0, 1)) = 0
		_CircleAmount ("Circle", Range(0, 1)) = 0
		_VertexColor ("Vertex Color", Float) = 0
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
	Fallback "tk2d/BlendVertexColor"
	//CustomEditor "EmissiveToggleInspector"
}
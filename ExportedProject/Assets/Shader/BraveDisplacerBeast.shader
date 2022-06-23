Shader "Brave/DisplacerBeast" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NoiseTex ("Noise", 2D) = "white" {}
		_SpaceTex ("Space", 2D) = "white" {}
		[Toggle] _Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_OutlineColor ("Outline Color", Vector) = (0,0,0,1)
		_EmissivePower ("Emissive Power", Float) = 0
		_EmissiveColorPower ("Emissive Color Power", Float) = 7
		_EmissiveGlowToggle ("Custom Emissive Mode", Float) = 0
		_OverrideColor ("Tint Color", Vector) = (1,1,1,0)
		_ValueMaximum ("Max Value", Float) = 1
		_ValueMinimum ("Min value", Float) = 0.5
		_BurnAmount ("Burn", Range(0, 1)) = 0
		_RectangleAmount ("Rectangle", Range(0, 1)) = 0
		_CircleAmount ("Circle", Range(0, 1)) = 0
		_DisplacerFade ("Displacer Fade", Range(0, 1.5)) = 0
		_DisplacerThreshold ("Displacer Threshold", Float) = 0.5
		_ScrollSpeed ("Scroll Speed", Float) = 1
		_SpaceMultiplier ("Space Multiplier", Float) = 1
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
	Fallback "VertexLit"
	//CustomEditor "EmissiveToggleInspector"
}
Shader "Brave/Effects/TitleInk" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_OverrideColor ("Color", Vector) = (1,1,1,1)
		_StepValue ("Step Value", Float) = 0.1
		_SkyBoost ("Sky Boost", Float) = 0.5
		_SkyPower ("Sky Power", Float) = 1
		_DitherCohesionFactor ("Cohesion Factor", Float) = 0.5
		_SpaceVector ("Space Vector", Vector) = (50,27,150,133)
		_CurvePower ("CP", Float) = 0
		_CurveFreq ("Curve Freq", Float) = 1
		_CurveColorFactor ("CCF", Float) = 1
		_Wavy ("Wavy", Float) = 1
		_WavePower ("Wave Power", Float) = 1
		_Emission ("Emiss", Float) = 0
		_AlphaMod ("Alpha", Float) = 1
		_RadialFade ("RadialFade", Range(0, 2)) = 0
		_YThreshold ("Y Threshold", Float) = -128.875
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
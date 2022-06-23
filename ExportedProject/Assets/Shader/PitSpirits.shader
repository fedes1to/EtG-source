Shader "Brave/Internal/PitSpirits" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_MaxValue ("Max Value", Float) = 1
		_OverrideColor ("Spirit Color", Vector) = (1,0,0,1)
		_NoiseTex ("Noise", 2D) = "white" {}
		_StepValue ("Step Value", Float) = 0.1
		_SkyBoost ("Sky Boost", Float) = 0.5
		_SkyPower ("Sky Power", Float) = 1
		_DitherCohesionFactor ("Cohesion Factor", Float) = 0.5
		_CurvePower ("CP", Float) = 0
		_CurveFreq ("Curve Freq", Float) = 1
		_CurveColorFactor ("CCF", Float) = 1
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
	Fallback "Brave/LitTk2dCustomFalloffCutout"
}
Shader "Brave/MeduziWaterCaustics" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "white" {}
		_CausticTex ("Casutics", 2D) = "white" {}
		_CausticScale ("Caustic Scale", Float) = 1
		_NoiseTex ("Noise", 2D) = "white" {}
		_CausticColor ("Caustic Color", Vector) = (1,1,1,0)
		_LightCausticPower ("LCP", Float) = 0.125
		_LightCausticColor ("Light Color", Vector) = (1,1,1,1)
		_DarkCausticPower ("DCP", Float) = 0.08
		_DarkCausticColor ("Dark Color", Vector) = (0,0,0,1)
		_ValueMinimum ("Value Min", Float) = 0.5
		_ValueMaximum ("Value Max", Float) = 0.5
		_EmissiveBoost ("Boost", Float) = 0
		_TimeScale ("TimeScale", Float) = 1
		_ReflPower ("Refl Power", Float) = 1
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
	Fallback "Transparent/Cutout/Diffuse"
}
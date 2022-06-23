Shader "Brave/Internal/FinalGunRoom_BG_02" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Octaves ("Octaves", Float) = 8
		_Frequency ("Frequency", Float) = 1
		_Amplitude ("Amplitude", Float) = 1
		_Lacunarity ("Lacunarity", Float) = 1.92
		_Lacuna2 ("Lacuna 2", Float) = 1
		_Persistence ("Persistence", Float) = 0.8
		_SteppyStep ("Step", Float) = 0.1
		_Offset ("Offset", Vector) = (0,0,0,0)
		_ThunderProgress ("Thunder Progress", Range(0, 1)) = 0
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
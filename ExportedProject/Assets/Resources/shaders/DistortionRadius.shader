Shader "Brave/Internal/DistortionRadius" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_WaveCenter ("Wave Center", Vector) = (0,0,0,0)
		_Strength ("Strength", Float) = 0.01
		_TimePulse ("Time Factor", Range(0, 1)) = 0
		_RadiusFactor ("Radius Factor", Float) = 1
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
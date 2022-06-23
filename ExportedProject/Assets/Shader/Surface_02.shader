Shader "Kvant/Tunnel/Surface_02" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_StepFactor ("Attenstep", Float) = 4
		_Displacement ("Disp", Float) = 0
		_DesatPhase ("Desat Phase", Float) = 0
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
	Fallback "Diffuse"
}
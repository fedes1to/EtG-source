Shader "Hidden/DarknessChallengeShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_ConeAngle ("Cone Angle", Float) = 30
		_FadeRange ("Fade Angle", Float) = 10
		_Player1ScreenPosition ("Player 1 Pos", Vector) = (0.5,0.5,0,0)
		_Player2ScreenPosition ("Player 2 Pos", Vector) = (0.5,0.5,0,0)
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
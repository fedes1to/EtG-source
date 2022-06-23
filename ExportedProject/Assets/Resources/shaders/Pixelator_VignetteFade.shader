Shader "Brave/CameraEffects/Pixelator_VignetteFade" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_GBuffer ("G Buffer", 2D) = "white" {}
		_OcclusionMap ("OcclusionMap", 2D) = "white" {}
		_OcclusionUV ("OcclusionUV", Vector) = (0,0,0,0)
		_OcclusionFallbackColor ("Occlusion Fallback Color", Vector) = (0,0,0,1)
		_VignettePower ("VignettePower", Range(0, 6)) = 5.5
		_VignetteColor ("VignetteColor", Vector) = (0,0,0,1)
		_Saturation ("Saturation", Range(0, 1)) = 1
		_SaturationColor ("Sat Color", Vector) = (1,1,1,1)
		_Fade ("Fade", Range(0, 1)) = 1
		_FadeColor ("FadeColor", Vector) = (0,0,0,1)
		_LetterboxFrac ("Letterbox", Range(0, 0.5)) = 0.5
		_WindowboxFrac ("Windowbox", Range(0, 0.5)) = 0.5
		_DamagedTex ("Damaged Vignette", 2D) = "black" {}
		_DamagedPower ("Damaged Power", Range(0, 1)) = 0
		_GammaGamma ("Gamma", Float) = 1
		_LowlightColor ("Lowlight Color", Vector) = (1,0,-1,0)
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
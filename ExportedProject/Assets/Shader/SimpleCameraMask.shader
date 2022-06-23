Shader "Brave/Effects/SimpleCameraMask" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BGTex ("BG (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "white" {}
		_MinimapMaskTexPng ("Minimap Mask (R)", 2D) = "white" {}
		_Fade ("Fade", Range(0, 1)) = 1
		_BGTexUV ("BGTexUV", Vector) = (0,0,0,0)
		_CameraRect ("CameraRect", Vector) = (0,0,0,0)
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
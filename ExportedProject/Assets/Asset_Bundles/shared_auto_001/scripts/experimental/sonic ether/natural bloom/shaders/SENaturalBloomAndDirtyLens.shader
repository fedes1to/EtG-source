Shader "Hidden/SENaturalBloomAndDirtyLens" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bloom0 ("Bloom0 (RGB)", 2D) = "black" {}
		_Bloom1 ("Bloom1 (RGB)", 2D) = "black" {}
		_Bloom2 ("Bloom2 (RGB)", 2D) = "black" {}
		_Bloom3 ("Bloom3 (RGB)", 2D) = "black" {}
		_Bloom4 ("Bloom4 (RGB)", 2D) = "black" {}
		_Bloom5 ("Bloom5 (RGB)", 2D) = "black" {}
		_LensDirt ("Lens Dirt", 2D) = "black" {}
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
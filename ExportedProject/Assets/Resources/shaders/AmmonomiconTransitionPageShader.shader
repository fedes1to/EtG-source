Shader "Custom/AmmonomiconTransitionPageShader" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_TopBezier ("Top Bezier", Vector) = (0,0,0,0)
		_LeftBezier ("Left Bezier", Vector) = (0,0,0,0)
		_BottomBezier ("Bottom Bezier", Vector) = (1,1,1,1)
		_RightBezier ("Right Bezier", Vector) = (1,1,1,1)
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
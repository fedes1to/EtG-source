Shader "Brave/Internal/RatFloorShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise", 2D) = "white" {}
		_FloorTex ("Floor", 2D) = "white" {}
		_EdgeColor ("Edge Color", Vector) = (0.5,0.1,0.7,1)
		_BorderColor ("Border Color", Vector) = (0,0,0,1)
		_RotSpeed ("Rotation Speed", Float) = 0
		_Magnitudes ("LS/SS MinMax", Vector) = (0.25,0.25,0.1,0.1)
		_LSFreq ("Large-Scale Frequency", Float) = 8
		_SSFreq ("Small-Scale Frequency", Float) = 24
		_PlayerPos ("Player Position World", Vector) = (0,0,0,0)
		_HoleEdgeDepth ("Hole Edge Depth", Float) = 6
		_UVDistCutoff ("Cutoff Edge", Float) = 0.4
		_TimeScale ("Time Scale", Float) = 1
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
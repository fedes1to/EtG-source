Shader "Brave/CutoutEmissiveGodrayExperimental" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_EmissivePower ("Emissive Power", Float) = 0
		_EmissiveColorPower ("Emissive Color Power", Float) = 7
		_EmissiveColor ("Emissive Color", Vector) = (1,1,1,1)
		_LightScale ("Light Scale", Float) = 1.5
		_HeightOffGround ("HeightOffGround", Float) = 10
		_MeshBoundsCenter ("Mesh Bounds", Vector) = (0,0,0,0)
		_MeshBoundsExtents ("Mesh Extents", Vector) = (1,1,1,1)
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
	Fallback "VertexLit"
}
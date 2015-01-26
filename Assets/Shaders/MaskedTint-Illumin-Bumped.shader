Shader "Tinted by Alpha Mask/Self-Illumin/Bumped Diffuse" {
Properties {
	_Color ("Tinted Color", Color) = (1,1,1)
	_MainTex ("Base (RGB) Tinting Mask & Gloss (A)", 2D) = "white" {}
	_Illum ("Illumin (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_EmissionLM ("Emission (Lightmapper)", Float) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 300

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Illum;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_Illum;
	float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Alpha = c.a;
	c = c * c.a + _Color * (1 - c.a);
	o.Albedo = c.rgb;
	o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum).a;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
} 
FallBack "Self-Illumin/Diffuse"
}

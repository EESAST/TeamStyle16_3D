Shader "Tinted by Alpha Mask/Bumped Diffuse" {
Properties {
	_Color ("Tinted Color", Color) = (1,1,1)
	_MainTex ("Base (RGB) Tinting Mask (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 300

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Alpha = c.a;
	c = c * c.a + _Color * (1 - c.a);
	o.Albedo = c.rgb;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG  
}

FallBack "Diffuse"
}

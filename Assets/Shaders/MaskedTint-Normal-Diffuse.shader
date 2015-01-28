Shader "Tinted by Alpha Mask/Diffuse" {
Properties {
	_Color ("Tinted Color", Color) = (1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Alpha = c.a;
	c = c * c.a + _Color * (1 - c.a);
	o.Albedo = c.rgb;
}
ENDCG
}

Fallback "VertexLit"
}

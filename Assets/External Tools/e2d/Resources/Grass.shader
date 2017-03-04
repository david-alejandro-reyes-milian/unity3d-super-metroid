/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.
///
/// The shader renders only single bush of grass waving it according to the parameters.
/// The wave speed is the same for all bushes but it is offset based on the position of grass.
/// Custom data are passed through the color bindings:
///  - grass ID
///  - wave amplitude / 10
///  - wave offset / 10
///  - nothing (0)
/// Currently the shader supports up to 4 textures but this could be extended by adding more texture parameters.

Shader "e2d/Grass" {
Properties {
	_Grass0 ("Grass 0 (R)", 2D) = "white" {}
	_Grass1 ("Grass 1 (G)", 2D) = "white" {}
	_Grass2 ("Grass 2 (B)", 2D) = "white" {}
	_Grass3 ("Grass 3 (A)", 2D) = "white" {}
	_WaveFrequency ("WaveFrequency", Float) = 1
	
	// used in fallback on old cards
	_MainTex ("Fallback Texture (RGB)", 2D) = "white" {}
	_Color ("Fallback Color", Color) = (1,1,1,1)
}
	
SubShader {
	Tags {
		"Queue" = "Transparent+100"
		"IgnoreProjector"="False"
		"RenderType" = "Transparent"
	}
	
CGPROGRAM
#pragma surface surf Lambert alpha vertex:vert
struct Input {
	float2 uv_Grass0 : TEXCOORD0;
	float4 color : COLOR;
};

sampler2D _Grass0, _Grass1, _Grass2, _Grass3;
float _WaveFrequency;

float round(float x)
{
	return sign(x)*floor(abs(x)+0.5);
}

void vert(inout appdata_full v)
{
	float2 waveDir = v.texcoord1.xy;
	float waveAmplitude = v.color.y * 10;
	float offset = v.color.z * 10;

	float wind = waveAmplitude * sin(_WaveFrequency * 2 * 3.14 * _Time.y + offset);
	v.vertex.xy += wind * (v.texcoord.y > 0.5) * waveDir;
}

void surf (Input IN, inout SurfaceOutput o)
{
	float2 uv = IN.uv_Grass0;
	float type = round(IN.color.x * 100);
	
	half4 color = half4(0, 0, 0, 0);
	color += (type==0) * tex2D(_Grass0, uv).rgba;
	color += (type==1) * tex2D(_Grass1, uv).rgba;
	color += (type==2) * tex2D(_Grass2, uv).rgba;
	color += (type==3) * tex2D(_Grass3, uv).rgba;
	o.Albedo = color.rgb;
	o.Alpha = color.a;
}

ENDCG  
}

// Fallback to Diffuse
Fallback "Diffuse"
}

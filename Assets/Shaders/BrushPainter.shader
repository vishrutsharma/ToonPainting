Shader "Custom/BrushPainter" {

	Properties{
        _Color("Color",Color) = (1,1,1,1)
		_MainTex("Explored Texture", 2D) = "white" {}
		_PaintMap("PaintMap", 2D) = "white" {} // texture to paint on
		_Brightness("Brightness",Range(0,1)) = 0.2
		_SampleStrength("SampleStrength",Range(0,1)) = 0.2
		_Strength("Strenght",Range(0,1)) = 0.2
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase"  "Queue" = "Geometry" }

		Pass{
		Lighting On
		
		ZWrite On
		ZTest Less
		
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag


#include "UnityCG.cginc"
#include "AutoLight.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
		float3 worldNormal : NORMAL;
		float3 viewDir : TEXCOORD2;

	};

	struct appdata {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float2 texcoord1 : TEXCOORD1;
		float3 normal:NORMAL;

	};

	sampler2D _PaintMap;
	sampler2D _MainTex;
	float4 _AmbientColor;
    float4 _Color;
	float4 _MainTex_ST;
	float _Brightness;
	float _SampleStrength;
	float _Strength;
	
	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv0 = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.viewDir = WorldSpaceViewDir(v.vertex);
		o.uv1 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;// lightmap uvs
		o.worldNormal = UnityObjectToWorldNormal(v.normal);
		return o;
	}

	half4 frag(v2f o) : COLOR{
		half4 main_color = tex2D(_MainTex, o.uv0); // main texture
		half4 paint = (tex2D(_PaintMap, o.uv1)); // painted on texture
		float4 endresult = main_color * paint;
		float dotProduct = max(0,dot(normalize(o.worldNormal),normalize(_WorldSpaceLightPos0.xyz)));
		dotProduct = floor(dotProduct/_SampleStrength);

		endresult =  endresult *dotProduct *_Strength +_Brightness ;
		return endresult;
		
	}
		ENDCG
	}
	}
}
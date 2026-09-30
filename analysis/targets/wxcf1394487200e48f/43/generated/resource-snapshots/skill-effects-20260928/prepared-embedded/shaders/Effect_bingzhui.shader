// Original compiled GLES and fixed state: generated/resource-snapshots/skill-effects-20260928/embedded-shader-evidence/Effect_bingzhui.json
Shader "AreaBattle/Recovered Effect/bingzhui" { Properties {
_ZwriteCtrl("ZwriteCtrl",Float)=0.0
_CullModeCtrl("CullModeCtrl",Float)=0.0
_SrcAlpha("SrcAlpha",Float)=5.0
_DesValue("DesValue",Float)=1.0
[HDR]_Texture_Color("Texture _Color ",Color)=(1.0,1.0,1.0,1.0)
_Texture("Texture ",2D)="white" {}
_Texture_uv("Texture_uv",Vector)=(0.0,0.0,0.0,0.0)
_Texture_noise("Texture_noise",2D)="white" {}
_Texture_noiseuv("Texture_noiseuv",Vector)=(0.0,0.0,0.0,0.0)
_Texture_noise_int("Texture_noise_int",Float)=0.0
[HDR]_Fresnel_Color("Fresnel_Color ",Color)=(1.0,1.0,1.0,1.0)
_Fresnel_scale("Fresnel_scale",Float)=1.0
_Fresnel_power("Fresnel_power",Float)=5.0
_colortex("colortex",2D)="white" {}
_Alpha("Alpha",Float)=1.0
[HideInInspector]_texcoord("",2D)="white" {}
[HideInInspector]__dirty("",Float)=1.0
} SubShader { Tags { "IGNOREPROJECTOR"="true" "IsEmissive"="true" "QUEUE"="Overlay+0" "RenderType"="Transparent" }
Pass { Name "FORWARD" Tags { "LightMode"="ForwardBase" } Blend [_SrcAlpha] [_DesValue] ZWrite [_ZwriteCtrl] Cull [_CullModeCtrl] ZTest LEqual
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
sampler2D _Texture_noise,_Texture,_colortex;float4 _Texture_noise_ST,_Texture_ST,_colortex_ST,_texcoord_ST,_Texture_Color,_Fresnel_Color;float2 _Texture_noiseuv,_Texture_uv;float _Texture_noise_int,_Fresnel_scale,_Fresnel_power,_Alpha;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw; return o;}
float4 sourceColor(v2f i){float noise=tex2D(_Texture_noise,i.uv.xy*_Texture_noise_ST.xy+_Texture_noise_ST.zw+_RecoveredEffectTime*_Texture_noiseuv).r;
float4 main=tex2D(_Texture,i.uv.xy*_Texture_ST.xy+_Texture_ST.zw+noise*_Texture_noise_int+_RecoveredEffectTime*_Texture_uv)*_Texture_Color*i.color;
float rim=pow(1-dot(i.normal,normalize(_WorldSpaceCameraPos-i.world)),_Fresnel_power)*_Fresnel_scale;
return float4(main.rgb*tex2D(_colortex,i.uv.xy*_colortex_ST.xy+_colortex_ST.zw).rgb+rim*_Fresnel_Color.rgb,main.a*_Alpha);}
float4 frag(v2f i):SV_Target{float noise=tex2D(_Texture_noise,i.uv.xy*_Texture_noise_ST.xy+_Texture_noise_ST.zw+_RecoveredEffectTime*_Texture_noiseuv).r;
float4 main=tex2D(_Texture,i.uv.xy*_Texture_ST.xy+_Texture_ST.zw+noise*_Texture_noise_int+_RecoveredEffectTime*_Texture_uv)*_Texture_Color*i.color;
float rim=pow(1-dot(i.normal,normalize(_WorldSpaceCameraPos-i.world)),_Fresnel_power)*_Fresnel_scale;
return float4(main.rgb*tex2D(_colortex,i.uv.xy*_colortex_ST.xy+_colortex_ST.zw).rgb+rim*_Fresnel_Color.rgb,main.a*_Alpha);}
ENDCG
}
Pass { Name "FORWARD" Tags { "LightMode"="ForwardAdd" } Blend One One ZWrite Off Cull [_CullModeCtrl] ZTest LEqual
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
sampler2D _Texture_noise,_Texture,_colortex;float4 _Texture_noise_ST,_Texture_ST,_colortex_ST,_texcoord_ST,_Texture_Color,_Fresnel_Color;float2 _Texture_noiseuv,_Texture_uv;float _Texture_noise_int,_Fresnel_scale,_Fresnel_power,_Alpha;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw; return o;}
float4 sourceColor(v2f i){float noise=tex2D(_Texture_noise,i.uv.xy*_Texture_noise_ST.xy+_Texture_noise_ST.zw+_RecoveredEffectTime*_Texture_noiseuv).r;
float4 main=tex2D(_Texture,i.uv.xy*_Texture_ST.xy+_Texture_ST.zw+noise*_Texture_noise_int+_RecoveredEffectTime*_Texture_uv)*_Texture_Color*i.color;
float rim=pow(1-dot(i.normal,normalize(_WorldSpaceCameraPos-i.world)),_Fresnel_power)*_Fresnel_scale;
return float4(main.rgb*tex2D(_colortex,i.uv.xy*_colortex_ST.xy+_colortex_ST.zw).rgb+rim*_Fresnel_Color.rgb,main.a*_Alpha);}
float4 frag(v2f i):SV_Target{float4 result=sourceColor(i);return float4(0,0,0,result.a);}
ENDCG
}
} }
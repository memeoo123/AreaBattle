// Original compiled GLES and fixed state: generated/resource-snapshots/skill-effects-20260928/embedded-shader-evidence/Effect_Fire_Ruan_Dissolves_Noise.json
Shader "AreaBattle/Recovered Effect/Fire Ruan Dissolves Noise" { Properties {
_UVOffset("UVOffset",Float)=0.0
[HDR]_MainColor("Main Color",Color)=(2.0,2.0,2.0,1.0)
_MainTex("Main Tex",2D)="white" {}
_MainSpeed_X("Main Speed_X",Float)=0.0
_MainSpeed_Y("Main Speed_Y",Float)=0.0
_MainTexTiling_xyOffset_zw("Main Tex Tiling_xy Offset_zw",Vector)=(1.0,1.0,0.0,0.0)
_MaskTexture("Mask Texture ",2D)="white" {}
_MaskX("MaskX",Float)=0.0
_MaskY("MaskY",Float)=0.0
_MaskTexTiling_xyOffset_zw("Mask Tex Tiling_xy Offset_zw",Vector)=(1.0,1.0,0.0,0.0)
_HuoDissolutionTexture("Huo Dissolution Texture",2D)="white" {}
_HuoDissolutionTexturespeedx("Huo Dissolution Texture speed x",Float)=-1.0
_HuoDissolutionTexturespeedy("Huo Dissolution Texture speed y",Float)=-1.0
[HDR]_EdgeColor("EdgeColor",Color)=(1.0,1.0,1.0,1.0)
_Edge("Edge",Range(0.0,1.0))=0.0
_DissolvesTexture("Dissolves Texture",2D)="white" {}
_DissolvesTexture1("Dissolves Texture Tiling Offset",Vector)=(1.0,1.0,0.0,0.0)
_Hardness("Hardness",Range(0.0,0.9900000095367432))=0.9900000095367432
_Dissolves("Dissolves",Range(0.0,1.0))=0.7250465750694275
_DissolvesMode("Dissolves Mode",Float)=0.0
_NoiseTexture("Noise Texture",2D)="white" {}
_NoiseTexTiling_xyOffset_zw("Noise Tex Tiling_xy Offset_zw",Vector)=(1.0,1.0,0.0,0.0)
_Noise("Noise",Range(0.0,1.0))=0.0
_NoiseSpendX("Noise Spend X",Float)=0.0
_NoiseSpendY("Noise Spend Y",Float)=0.0
} SubShader { Tags { "QUEUE"="Transparent" "RenderType"="Transparent" }
Pass { Name "Unlit" Tags { "LightMode"="ForwardBase" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off ZTest LEqual
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
sampler2D _NoiseTexture,_MainTex,_DissolvesTexture,_MaskTexture,_HuoDissolutionTexture;
float4 _NoiseTexTiling_xyOffset_zw,_MainTexTiling_xyOffset_zw,_DissolvesTexture1,_MaskTexTiling_xyOffset_zw,_HuoDissolutionTexture_ST,_EdgeColor,_MainColor;
float _NoiseSpendX,_NoiseSpendY,_DissolvesMode,_Dissolves,_Edge,_Hardness,_UVOffset,_MainSpeed_X,_MainSpeed_Y,_Noise,_MaskX,_MaskY,_HuoDissolutionTexturespeedx,_HuoDissolutionTexturespeedy;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;o.uv.zw=v.uv.xy*_NoiseTexTiling_xyOffset_zw.xy+_NoiseTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*float2(_NoiseSpendX,_NoiseSpendY);
float3 d=lerp(float3(_Dissolves,_Edge,_Hardness),v.uv1.xyz,_DissolvesMode);o.thresholds=(2-d.z)*float2(d.x*(1+d.y),d.x*(1+d.y)-d.y); return o;}
float4 sourceColor(v2f i){float2 uv=i.uv.xy*_MainTexTiling_xyOffset_zw.xy+_MainTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*_UVOffset*float2(_MainSpeed_X,_MainSpeed_Y);
uv+=(tex2D(_NoiseTexture,i.uv.zw).r-.5)*lerp(_Noise,i.custom.w,_DissolvesMode);
float4 main=tex2D(_MainTex,uv)*_MainColor*i.color;
float d=tex2D(_DissolvesTexture,i.uv.xy*_DissolvesTexture1.xy+_DissolvesTexture1.zw).r+1;
float hardness=lerp(_Hardness,i.custom.z,_DissolvesMode);float2 f=saturate((d-i.thresholds-hardness)/(1-hardness));
float mask=tex2D(_MaskTexture,i.uv.xy*_MaskTexTiling_xyOffset_zw.xy+_MaskTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*float2(_MaskX,_MaskY)).r;
float fire=tex2D(_HuoDissolutionTexture,i.uv.xy*_HuoDissolutionTexture_ST.xy+_HuoDissolutionTexture_ST.zw+_RecoveredEffectTime*float2(_HuoDissolutionTexturespeedx,_HuoDissolutionTexturespeedy)).r;
return float4(lerp(_EdgeColor.rgb,main.rgb,f.x),step(i.uv.y,fire)*saturate(f.y*main.a*mask));}
float4 frag(v2f i):SV_Target{float2 uv=i.uv.xy*_MainTexTiling_xyOffset_zw.xy+_MainTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*_UVOffset*float2(_MainSpeed_X,_MainSpeed_Y);
uv+=(tex2D(_NoiseTexture,i.uv.zw).r-.5)*lerp(_Noise,i.custom.w,_DissolvesMode);
float4 main=tex2D(_MainTex,uv)*_MainColor*i.color;
float d=tex2D(_DissolvesTexture,i.uv.xy*_DissolvesTexture1.xy+_DissolvesTexture1.zw).r+1;
float hardness=lerp(_Hardness,i.custom.z,_DissolvesMode);float2 f=saturate((d-i.thresholds-hardness)/(1-hardness));
float mask=tex2D(_MaskTexture,i.uv.xy*_MaskTexTiling_xyOffset_zw.xy+_MaskTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*float2(_MaskX,_MaskY)).r;
float fire=tex2D(_HuoDissolutionTexture,i.uv.xy*_HuoDissolutionTexture_ST.xy+_HuoDissolutionTexture_ST.zw+_RecoveredEffectTime*float2(_HuoDissolutionTexturespeedx,_HuoDissolutionTexturespeedy)).r;
return float4(lerp(_EdgeColor.rgb,main.rgb,f.x),step(i.uv.y,fire)*saturate(f.y*main.a*mask));}
ENDCG
}
} }
// Source: generated/resource-snapshots/skill-effects-20260928/shader-evidence/Effect_Pingzi.json; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered Effect/Pingzi" { Properties {
[HDR]_TintCd1("TintCd",Color)=(1.0,1.0,1.0,1.0)
_Main_Tex1("Main_Tex",2D)="white" {}
_TexPanner1("TexPanner",Vector)=(0.0,0.0,0.0,0.0)
_CdMult1("CdMult",Float)=10.0
_CdPower1("CdPower",Float)=0.0
_Edge_Power1("Edge_Power",Range(0.0,5.0))=0.0
[HDR]_EdgeCD1("EdgeCD",Color)=(1.0,1.0,1.0,1.0)
_Edgel_Mult1("Edgel_Mult",Range(0.0,1.0))=0.0
_AlphaPower1("AlphaPower",Float)=0.0
_HDDJ("HD-DJ",2D)="white" {}
[HideInInspector]_texcoord("",2D)="white" {}
} SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" }
Pass { Name "Unlit" Tags { "LightMode"="ForwardBase" } Blend SrcAlpha OneMinusSrcAlpha ZWrite On ZTest LEqual Cull Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;float4 uv2:TEXCOORD2;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};
sampler2D _Main_Tex1,_HDDJ;float4 _HDDJ_ST,_TintCd1,_EdgeCD1;float2 _TexPanner1;float _CdMult1,_CdPower1,_Edge_Power1,_Edgel_Mult1,_AlphaPower1;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color; return o;}

float4 frag(v2f i):SV_Target { float3 c=tex2D(_Main_Tex1,i.uv.xy+_RecoveredEffectTime*_TexPanner1).rgb*_CdMult1*_TintCd1.rgb;float rim=1-min(abs(dot(normalize(_WorldSpaceCameraPos-i.world),i.normal)),1);return float4(c*pow(rim,_CdPower1)+pow(rim,_Edge_Power1)*_EdgeCD1.rgb*_Edgel_Mult1+tex2D(_HDDJ,i.uv.xy*_HDDJ_ST.xy+_HDDJ_ST.zw).rgb,pow(rim,_AlphaPower1)*i.color.a); }
ENDCG
} } }

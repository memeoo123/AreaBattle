// Source: generated/resource-snapshots/skill-effects-20260928/shader-evidence/Effect_Sword.json; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered Effect/Sword" { Properties {
[HDR]_TintCd("TintCd",Color)=(1.0,1.0,1.0,1.0)
_Main_Tex("Main_Tex",2D)="white" {}
_TexPanner("TexPanner",Vector)=(0.0,0.0,0.0,0.0)
_CdMult("CdMult",Float)=10.0
_CdPower("CdPower",Float)=0.0
_Edge_Power("Edge_Power",Range(0.0,5.0))=0.0
[HDR]_EdgeCD("EdgeCD",Color)=(1.0,1.0,1.0,1.0)
_Edgel_Mult("Edgel_Mult",Range(0.0,1.0))=0.0
_AlphaPower("AlphaPower",Float)=0.0
} SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" }
Pass { Name "Unlit" Tags { "LightMode"="ForwardBase" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;float4 uv2:TEXCOORD2;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};
sampler2D _Main_Tex;float4 _Main_Tex_ST,_TintCd,_EdgeCD;float2 _TexPanner;float _CdMult,_CdPower,_Edge_Power,_Edgel_Mult,_AlphaPower;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color; return o;}

float4 frag(v2f i):SV_Target { float3 c=tex2D(_Main_Tex,i.uv.xy*_Main_Tex_ST.xy+_Main_Tex_ST.zw+_RecoveredEffectTime*_TexPanner).rgb*_CdMult*_TintCd.rgb;float rim=1-min(abs(dot(normalize(_WorldSpaceCameraPos-i.world),i.normal)),1);return float4(c*pow(rim,_CdPower)+pow(rim,_Edge_Power)*_EdgeCD.rgb*_Edgel_Mult,pow(rim,_AlphaPower)*i.color.a); }
ENDCG
} } }

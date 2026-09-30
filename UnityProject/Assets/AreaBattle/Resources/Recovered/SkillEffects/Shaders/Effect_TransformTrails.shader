// Source: generated/resource-snapshots/skill-effects-20260928/shader-evidence/Effect_TransformTrails.json; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered Effect/TransformTrails" { Properties {
[HDR]_TintCd1("TintCd",Color)=(1.0,1.0,1.0,1.0)
_Panner("Panner",Vector)=(0.0,0.0,0.0,0.0)
_Main_Tex2("Main_Tex2",2D)="white" {}
_Noise_Tex01("Noise_Tex01",2D)="white" {}
_Nosie_Mult2("Nosie_Mult",Range(0.0,1.0))=0.0
_Alpha_Mult1("Alpha_Mult",Range(0.0,5.0))=1.0
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
sampler2D _Noise_Tex01,_Main_Tex2;float4 _Noise_Tex01_ST,_Main_Tex2_ST,_Panner,_TintCd1;float _Nosie_Mult2,_Alpha_Mult1;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color; return o;}

float4 frag(v2f i):SV_Target { float2 n=tex2D(_Noise_Tex01,i.uv.xy*_Noise_Tex01_ST.xy+_Noise_Tex01_ST.zw+_RecoveredEffectTime*_Panner.xy).xy;float2 uv=i.uv.xy*_Main_Tex2_ST.xy+_Main_Tex2_ST.zw+_RecoveredEffectTime*_Panner.zw+n*_Nosie_Mult2;return tex2D(_Main_Tex2,uv).r*_TintCd1*max(1-i.uv.x,9.99999975e-05)*_Alpha_Mult1*i.color; }
ENDCG
} } }

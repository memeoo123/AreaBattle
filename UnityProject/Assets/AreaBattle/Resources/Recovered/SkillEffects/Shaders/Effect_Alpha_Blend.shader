// Source: generated/resource-snapshots/skill-effects-20260928/shader-evidence/Effect_Alpha_Blend.json; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered Effect/Alpha Blend" { Properties {
[HDR]_MainColor("Main Color",Color)=(1.0,1.0,1.0,1.0)
_MainTex("Main Tex",2D)="white" {}
[HideInInspector]_texcoord("",2D)="white" {}
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
sampler2D _MainTex;float4 _MainTex_ST,_MainColor;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color; return o;}

float4 frag(v2f i):SV_Target { return tex2D(_MainTex,i.uv.xy*_MainTex_ST.xy+_MainTex_ST.zw)*_MainColor*i.color; }
ENDCG
} } }

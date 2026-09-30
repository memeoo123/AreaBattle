// Source: generated/resource-snapshots/skill-effects-20260928/shader-evidence/Effect_Alpha_Blend_UV.json; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered Effect/Alpha Blend UV" { Properties {
_Mode("Mode",Float)=1.0
_Move("Move",Float)=0.0
[HDR]_MainColor("Main Color",Color)=(1.0,1.0,1.0,1.0)
_MainTextureSpeedx("Main Texture Speed x",Float)=0.0
_MainTextureSpeedy("Main Texture Speed y",Float)=0.0
_MainTexture("Main Texture",2D)="white" {}
_MaskTextureSpeedx("Mask Texture Speed x",Float)=0.0
_MaskTextureSpeedy("Mask Texture Speed y",Float)=0.0
_MaskTexture("Mask Texture",2D)="white" {}
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
sampler2D _MainTexture,_MaskTexture;float4 _MainTexture_ST,_MaskTexture_ST,_MainColor;float _Move,_Mode,_MainTextureSpeedx,_MainTextureSpeedy,_MaskTextureSpeedx,_MaskTextureSpeedy;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.uv.xy=v.uv.xy*_MainTexture_ST.xy+_MainTexture_ST.zw+_RecoveredEffectTime*_Move*lerp(v.uv1.zw,float2(_MainTextureSpeedx,_MainTextureSpeedy),_Mode);o.uv.zw=v.uv.xy*_MaskTexture_ST.xy+_MaskTexture_ST.zw+_RecoveredEffectTime*_Move*lerp(v.uv2.zw,float2(_MaskTextureSpeedx,_MaskTextureSpeedy),_Mode); return o;}

float4 frag(v2f i):SV_Target { float4 c=tex2D(_MainTexture,i.uv.xy)*_MainColor*i.color;c.a*=tex2D(_MaskTexture,i.uv.zw).r;return c; }
ENDCG
} } }

// Original compiled GLES and fixed state: generated/resource-snapshots/skill-effects-20260928/embedded-shader-evidence/Effect_Blend.json
Shader "AreaBattle/Recovered Effect/Blend" { Properties {
_ZwriteCtrl1("ZwriteCtrl",Float)=0.0
_CullModeCtrl1("CullModeCtrl",Float)=0.0
_SrcAlpha1("SrcAlpha",Float)=5.0
_DesValue1("DesValue",Float)=1.0
[HDR]_Main_color("主贴图颜色",Color)=(1.0,1.0,1.0,1.0)
_Main_tex("主贴图",2D)="white" {}
_Vector0("主帖图流动方向",Vector)=(0.0,0.0,0.0,0.0)
_TextureSample0("扭曲帖图",2D)="white" {}
_Float1("扭曲强度",Range(0.0,1.0))=0.0
_Vector1("扭曲帖图流动方向",Vector)=(0.0,0.0,0.0,0.0)
_TextureSample1("遮罩图",2D)="white" {}
_TextureSample3("扭曲遮罩图",2D)="white" {}
_Keyword0("Dissolve",Float)=0.0
_TextureSample2("溶解图",2D)="white" {}
_Float4("软硬边溶解",Range(0.0,1.0))=0.0
} SubShader { Tags { "IsEmissive"="true" "QUEUE"="Overlay+0" "RenderType"="Transparent" }
Pass { Name "FORWARD" Tags { "LightMode"="ForwardBase" } Blend [_SrcAlpha1] [_DesValue1] ZWrite [_ZwriteCtrl1] Cull [_CullModeCtrl1] ZTest LEqual
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
sampler2D _TextureSample0,_TextureSample3,_Main_tex,_TextureSample1;float4 _Main_tex_ST,_TextureSample3_ST,_TextureSample1_ST,_texcoord_ST,_Main_color;float2 _Vector0,_Vector1;float _Float1;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw; return o;}
float4 sourceColor(v2f i){float2 uv=i.uv.xy*_Main_tex_ST.xy+_Main_tex_ST.zw;
float2 noise=tex2D(_TextureSample0,uv+_RecoveredEffectTime*_Vector1).rg*_Float1;
float mask=tex2D(_TextureSample3,i.uv.xy*_TextureSample3_ST.xy+_TextureSample3_ST.zw).r;
float4 c=tex2D(_Main_tex,uv+_RecoveredEffectTime*_Vector0+noise*mask)*_Main_color*i.color;
c.a*=tex2D(_TextureSample1,i.uv.xy*_TextureSample1_ST.xy+_TextureSample1_ST.zw).r;return c;}
float4 frag(v2f i):SV_Target{float2 uv=i.uv.xy*_Main_tex_ST.xy+_Main_tex_ST.zw;
float2 noise=tex2D(_TextureSample0,uv+_RecoveredEffectTime*_Vector1).rg*_Float1;
float mask=tex2D(_TextureSample3,i.uv.xy*_TextureSample3_ST.xy+_TextureSample3_ST.zw).r;
float4 c=tex2D(_Main_tex,uv+_RecoveredEffectTime*_Vector0+noise*mask)*_Main_color*i.color;
c.a*=tex2D(_TextureSample1,i.uv.xy*_TextureSample1_ST.xy+_TextureSample1_ST.zw).r;return c;}
ENDCG
}
Pass { Name "FORWARD" Tags { "LightMode"="ForwardAdd" } Blend One One ZWrite Off Cull [_CullModeCtrl1] ZTest LEqual
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
sampler2D _TextureSample0,_TextureSample3,_Main_tex,_TextureSample1;float4 _Main_tex_ST,_TextureSample3_ST,_TextureSample1_ST,_texcoord_ST,_Main_color;float2 _Vector0,_Vector1;float _Float1;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw; return o;}
float4 sourceColor(v2f i){float2 uv=i.uv.xy*_Main_tex_ST.xy+_Main_tex_ST.zw;
float2 noise=tex2D(_TextureSample0,uv+_RecoveredEffectTime*_Vector1).rg*_Float1;
float mask=tex2D(_TextureSample3,i.uv.xy*_TextureSample3_ST.xy+_TextureSample3_ST.zw).r;
float4 c=tex2D(_Main_tex,uv+_RecoveredEffectTime*_Vector0+noise*mask)*_Main_color*i.color;
c.a*=tex2D(_TextureSample1,i.uv.xy*_TextureSample1_ST.xy+_TextureSample1_ST.zw).r;return c;}
float4 frag(v2f i):SV_Target{float4 result=sourceColor(i);return float4(0,0,0,result.a);}
ENDCG
}
} }
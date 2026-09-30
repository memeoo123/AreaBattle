Shader "AreaBattle/Recovered SkeletonMeshBaker"
{
 Properties {
  _MainTex("Recovered atlas",2D)="white"{}
  _AnimTex("Recovered packed animation",2D)="black"{}
  _Color("Camp color",Color)=(.31,.41,.73,1)
  _AnimAdd("Animation minimum",Vector)=(0,0,0,0)
  _AnimMul("Animation extent",Vector)=(1,1,0,0)
  _AnimTime("Start row, frame span, inverse duration, start time",Vector)=(0,1,1,0)
  _AnimLoop("Loop",Float)=1
  _BattleVisualTime("Scaled presentation clock",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
  Pass {
   Cull Off ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex,_AnimTex;
   float4 _AnimTex_TexelSize,_AnimAdd,_AnimMul,_AnimTime,_Color;
   float _AnimLoop,_BattleVisualTime;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 anim:TEXCOORD1;float4 color:COLOR;};
   struct v2f {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   v2f vert(appdata v) {
    v2f o;
    float t=(_BattleVisualTime-_AnimTime.w)*_AnimTime.z;
    float phase=lerp(saturate(t),frac(t),_AnimLoop);
    float2 uv=float2(v.anim.x+.5,_AnimTime.x+.5+phase*_AnimTime.y)*_AnimTex_TexelSize.xy;
    float4 packed=tex2Dlod(_AnimTex,float4(uv,0,0));
    float2 xy=(packed.xz*65536.0+packed.yw)*_AnimMul.xy/65535.0+_AnimAdd.xy;
    o.position=UnityObjectToClipPos(float4(xy,_AnimAdd.z,1));o.uv=v.uv;o.color=v.color;return o;
   }
   float4 frag(v2f i):SV_Target {
    float4 s=tex2D(_MainTex,i.uv)*i.color;
    float a=smoothstep(.1,.5,length(s.rgb-float3(.308999985,.41170001,.729399979)));
    return lerp(_Color,s,a);
   }
   ENDCG
  }
 }
}

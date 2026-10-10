Shader "AreaBattle/WarmStoneWall"
{
 Properties { _Stone("Stone",Color)=(.96,.90,.78,1) _Mortar("Mortar",Color)=(.56,.49,.37,1) }
 SubShader {
  Tags {"RenderType"="Opaque" "Queue"="Geometry"}
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   float4 _Stone,_Mortar;
   struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
   struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);return o;}
   float4 frag(v2f i):SV_Target {
    float3 n=normalize(i.normal);float side=abs(n.x)>abs(n.z)?i.world.z:i.world.x;
    float row=floor(i.world.y/.034);float2 brick=float2(side/.072+fmod(row,2)*.5,i.world.y/.034);
    float2 cell=frac(brick);float2 dist=min(cell,1-cell);float joint=1-smoothstep(.025,.09,min(dist.x,dist.y));
    float variation=frac(sin(dot(floor(brick),float2(12.9898,78.233)))*43758.5453);
    float light=.9+.1*saturate(dot(n,normalize(float3(-.45,.85,-.35))));
    float3 stone=_Stone.rgb*(.94+.10*variation);
    float3 color=lerp(stone,_Mortar.rgb,joint*.65)*light;
    color=lerp(color,_Stone.rgb*1.07,saturate(n.y)*.85);
    return float4(color,1);
   }
   ENDCG
  }
 }
}

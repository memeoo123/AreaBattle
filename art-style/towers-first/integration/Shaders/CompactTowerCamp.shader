Shader "AreaBattle/CompactTowerCamp"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _CampColor("Camp",Color)=(.157,.522,1,1) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _CampColor;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {
    float4 c=tex2D(_MainTex,i.uv);
    float blue=min(c.b-c.r,c.b-c.g);
    float mask=smoothstep(.035,.16,blue);
    float light=max(c.r,max(c.g,c.b));
    float3 camp=_CampColor.rgb*light;
    // Preserve pale roof highlights while replacing only saturated blue areas.
    camp=lerp(camp,light.xxx,saturate(c.r/max(.001,c.b))*.25);
    c.rgb=lerp(c.rgb,camp,mask);return c*i.color;
   }
   ENDCG
  }
 }
}

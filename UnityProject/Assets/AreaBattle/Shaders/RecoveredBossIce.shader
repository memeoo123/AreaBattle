// Source shader tex (CAB-68a3634b9a99c5a2a10a36b772e4e945:262130982039568269).
// GLES: expanded front-culled outline, opaque RGB texture, zero additive light.
Shader "AreaBattle/Recovered Boss Ice tex" {
 Properties {
  _ASEOutlineColor("Outline Color",Color)=(.1886792,.1797942,.1717693,0)
  _ASEOutlineWidth("Outline Width",Float)=.005
  _TextureSample0("Texture Sample 0",2D)="white"{}
  [HideInInspector] _texcoord("",2D)="white"{}
  [HideInInspector] __dirty("",Float)=1
 }
 SubShader {
  Tags {"RenderType"="Opaque" "Queue"="Geometry" "IsEmissive"="true"}
  Pass {
   Name "FORWARD" Tags {"LightMode"="ForwardBase"} Cull Front ZWrite On ZTest LEqual Blend One Zero
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _ASEOutlineWidth;float4 _ASEOutlineColor;
   float4 vert(appdata_base v):SV_POSITION{return UnityObjectToClipPos(float4(v.vertex.xyz+v.normal*_ASEOutlineWidth,1));}
   float4 frag():SV_Target{return float4(_ASEOutlineColor.rgb,1);}
   ENDCG
  }
  Pass {
   Name "FORWARD" Tags {"LightMode"="ForwardBase"} Cull Back ZWrite On ZTest LEqual Blend One Zero
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _TextureSample0;float4 _TextureSample0_ST;
   struct vary{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
   vary vert(appdata_base v){vary o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;return o;}
   float4 frag(vary i):SV_Target{return float4(tex2D(_TextureSample0,i.uv*_TextureSample0_ST.xy+_TextureSample0_ST.zw).rgb,1);}
   ENDCG
  }
  Pass {
   Name "FORWARD" Tags {"LightMode"="ForwardAdd"} Cull Back ZWrite Off ZTest LEqual Blend One One
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float4 vert(appdata_base v):SV_POSITION{return UnityObjectToClipPos(v.vertex);}
   float4 frag():SV_Target{return float4(0,0,0,1);}
   ENDCG
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} Cull Back ZWrite On ZTest LEqual
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_shadowcaster
   #include "UnityCG.cginc"
   struct vary{V2F_SHADOW_CASTER;};
   vary vert(appdata_base v){vary o;TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);return o;}
   float4 frag(vary i):SV_Target{SHADOW_CASTER_FRAGMENT(i)}
   ENDCG
  }
 }
}

Shader "AreaBattle/WarmStoneWall"
{
 Properties { _Stone("Stone",Color)=(.96,.90,.78,1) _Mortar("Mortar",Color)=(.56,.49,.37,1) }
 SubShader { Tags {"RenderType"="Opaque" "Queue"="Geometry"}
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Stone;
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;};
 struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float4 color:COLOR;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color;return o;}
 float4 frag(v2f i):SV_Target{
  float3 n=normalize(i.normal);float d=saturate(dot(n,normalize(float3(-.45,.85,-.35))));
  float light=d>.72?1.02:(d>.32?.86:.62);
  return float4(GammaToLinearSpace(_Stone.rgb)*i.color.rgb*light,1);
 }
 ENDCG }
 }
}

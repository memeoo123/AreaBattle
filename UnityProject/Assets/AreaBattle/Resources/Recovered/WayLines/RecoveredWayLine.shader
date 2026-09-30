Shader "AreaBattle/Recovered WayLine" {
Properties {
 _Tex ("Tex",2D)="white" {} _Vspeed ("Vspeed",Float)=0 _Uspeed ("Uspeed",Float)=0
 [HDR] _color ("color",Color)=(1,1,1,1) _opacity ("opacity",Range(0,5))=3
 _BattleVisualTime ("Host scaled clock (-1 uses Unity time)",Float)=-1
}
SubShader { Tags { "IgnoreProjector"="True" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass { Name "FORWARD" Tags { "LightMode"="ForwardBase" } Cull Off ZWrite Off ZTest LEqual
 Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
 struct v2f { float4 pos:SV_POSITION; half2 uv:TEXCOORD0; };
 sampler2D _Tex; float4 _Tex_ST; half4 _color; half _Vspeed,_Uspeed,_opacity; float _BattleVisualTime;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 half4 frag(v2f i):SV_Target {
  float clock=_BattleVisualTime<0?_Time.y:_BattleVisualTime;
  float t=clock*4.99999987e-06; t=sign(t)*frac(abs(t))*200000.0;
  float2 uv=(i.uv+t*float2(_Vspeed,_Uspeed))*_Tex_ST.xy+_Tex_ST.zw;
  half4 c=tex2D(_Tex,uv);return half4(c.rgb*_color.rgb,saturate(c.a*_opacity));
 }
 ENDCG
 }
}
}
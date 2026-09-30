"""Translate the five original compiled GLES skill shaders into editable built-in HLSL."""
import json
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/skill-effects-20260928';O=S/'prepared/shaders';O.mkdir(parents=True,exist_ok=True)
common='''#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;float4 uv2:TEXCOORD2;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};
'''
vertex='''v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;VERTEX_BODY return o;}
'''
bodies={
'Effect/Alpha Blend':('sampler2D _MainTex;float4 _MainTex_ST,_MainColor;','', 'return tex2D(_MainTex,i.uv.xy*_MainTex_ST.xy+_MainTex_ST.zw)*_MainColor*i.color;'),
'Effect/Alpha Blend UV':('sampler2D _MainTexture,_MaskTexture;float4 _MainTexture_ST,_MaskTexture_ST,_MainColor;float _Move,_Mode,_MainTextureSpeedx,_MainTextureSpeedy,_MaskTextureSpeedx,_MaskTextureSpeedy;', 'o.uv.xy=v.uv.xy*_MainTexture_ST.xy+_MainTexture_ST.zw+_RecoveredEffectTime*_Move*lerp(v.uv1.zw,float2(_MainTextureSpeedx,_MainTextureSpeedy),_Mode);o.uv.zw=v.uv.xy*_MaskTexture_ST.xy+_MaskTexture_ST.zw+_RecoveredEffectTime*_Move*lerp(v.uv2.zw,float2(_MaskTextureSpeedx,_MaskTextureSpeedy),_Mode);','float4 c=tex2D(_MainTexture,i.uv.xy)*_MainColor*i.color;c.a*=tex2D(_MaskTexture,i.uv.zw).r;return c;'),
'Effect/TransformTrails':('sampler2D _Noise_Tex01,_Main_Tex2;float4 _Noise_Tex01_ST,_Main_Tex2_ST,_Panner,_TintCd1;float _Nosie_Mult2,_Alpha_Mult1;','','float2 n=tex2D(_Noise_Tex01,i.uv.xy*_Noise_Tex01_ST.xy+_Noise_Tex01_ST.zw+_RecoveredEffectTime*_Panner.xy).xy;float2 uv=i.uv.xy*_Main_Tex2_ST.xy+_Main_Tex2_ST.zw+_RecoveredEffectTime*_Panner.zw+n*_Nosie_Mult2;return tex2D(_Main_Tex2,uv).r*_TintCd1*max(1-i.uv.x,9.99999975e-05)*_Alpha_Mult1*i.color;'),
'Effect/Sword':('sampler2D _Main_Tex;float4 _Main_Tex_ST,_TintCd,_EdgeCD;float2 _TexPanner;float _CdMult,_CdPower,_Edge_Power,_Edgel_Mult,_AlphaPower;','','float3 c=tex2D(_Main_Tex,i.uv.xy*_Main_Tex_ST.xy+_Main_Tex_ST.zw+_RecoveredEffectTime*_TexPanner).rgb*_CdMult*_TintCd.rgb;float rim=1-min(abs(dot(normalize(_WorldSpaceCameraPos-i.world),i.normal)),1);return float4(c*pow(rim,_CdPower)+pow(rim,_Edge_Power)*_EdgeCD.rgb*_Edgel_Mult,pow(rim,_AlphaPower)*i.color.a);'),
'Effect/Pingzi':('sampler2D _Main_Tex1,_HDDJ;float4 _HDDJ_ST,_TintCd1,_EdgeCD1;float2 _TexPanner1;float _CdMult1,_CdPower1,_Edge_Power1,_Edgel_Mult1,_AlphaPower1;','','float3 c=tex2D(_Main_Tex1,i.uv.xy+_RecoveredEffectTime*_TexPanner1).rgb*_CdMult1*_TintCd1.rgb;float rim=1-min(abs(dot(normalize(_WorldSpaceCameraPos-i.world),i.normal)),1);return float4(c*pow(rim,_CdPower1)+pow(rim,_Edge_Power1)*_EdgeCD1.rgb*_Edgel_Mult1+tex2D(_HDDJ,i.uv.xy*_HDDJ_ST.xy+_HDDJ_ST.zw).rgb,pow(rim,_AlphaPower1)*i.color.a);')}
rows=[]
for info in json.loads((S/'shader-evidence/manifest.json').read_text()):
 tree=json.loads((R/info['tree']).read_text());form=tree['m_ParsedForm'];name=info['name'];props=[]
 for p in form['m_PropInfo']['m_Props']:
  d=[p[f'm_DefValue[{i}]'] for i in range(4)];typ=p['m_Type'];definition={0:'Color',1:'Vector',2:'Float',3:f'Range({d[1]},{d[2]})',4:'2D'}[typ]
  value='('+','.join(str(x) for x in d)+')' if typ in (0,1) else json.dumps(p['m_DefTexture']['m_DefaultName'])+' {}' if typ==4 else str(d[0])
  attrs='[HDR]' if p['m_Flags']&16 else ''
  if p['m_Flags']&1:attrs+='[HideInInspector]'
  props.append(f'{attrs}{p["m_Name"]}({json.dumps(p["m_Description"])},{definition})={value}')
 st=form['m_SubShaders'][0]['m_Passes'][0]['m_State'];assert st['rtBlend0']['srcBlend']['val']==5 and st['rtBlend0']['destBlend']['val']==10 and st['zTest']['val']==4 and st['culling']['val']==0
 uniforms,vert,frag=bodies[name];shader=f'''// Source: {info['tree']}; original GLES retained beside this artifact.
// _RecoveredEffectTime is the explicitly driven equivalent of original _Time.y.
Shader "AreaBattle/Recovered {name}" {{ Properties {{
{chr(10).join(props)}
}} SubShader {{ Tags {{ "Queue"="Transparent" "RenderType"="Transparent" }}
Pass {{ Name "Unlit" Tags {{ "LightMode"="ForwardBase" }} Blend SrcAlpha OneMinusSrcAlpha ZWrite {"On" if st['zWrite']['val'] else "Off"} ZTest LEqual Cull Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
{common}{uniforms}
{vertex.replace('VERTEX_BODY',vert)}
float4 frag(v2f i):SV_Target {{ {frag} }}
ENDCG
}} }} }}
''';path=O/(name.replace('/','_').replace(' ','_')+'.shader');path.write_text(shader,encoding='utf8');rows.append({'sourceName':name,'restoredName':'AreaBattle/Recovered '+name,'path':str(path.relative_to(R)).replace('\\','/'),'source':info,'state':{k:st[k] for k in ('rtBlend0','zWrite','zTest','culling')}})
(S/'prepared/shader-map.json').write_text(json.dumps(rows,indent=2),encoding='utf8');print('Prepared',len(rows),'source-formula shaders')

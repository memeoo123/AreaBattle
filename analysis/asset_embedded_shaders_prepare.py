"""Original GLES formula/state translation for three tower status shaders."""
import json
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/skill-effects-20260928';O=S/'prepared-embedded/shaders';O.mkdir(exist_ok=True)
read=lambda p:json.loads(p.read_text(encoding='utf8'))
common='''#include "UnityCG.cginc"
float _RecoveredEffectTime;
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;float4 uv:TEXCOORD0;float4 uv1:TEXCOORD1;};
struct v2f {float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float4 color:COLOR;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;float4 custom:TEXCOORD3;float2 thresholds:TEXCOORD4;};
'''
vertex='''v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=float4(v.uv.xy,0,0);o.color=v.color;o.custom=v.uv1;o.thresholds=0;VERTEX return o;}
'''
forms={
'Effect/bingzhui':('sampler2D _Texture_noise,_Texture,_colortex;float4 _Texture_noise_ST,_Texture_ST,_colortex_ST,_texcoord_ST,_Texture_Color,_Fresnel_Color;float2 _Texture_noiseuv,_Texture_uv;float _Texture_noise_int,_Fresnel_scale,_Fresnel_power,_Alpha;',
'o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw;',
'''float noise=tex2D(_Texture_noise,i.uv.xy*_Texture_noise_ST.xy+_Texture_noise_ST.zw+_RecoveredEffectTime*_Texture_noiseuv).r;
float4 main=tex2D(_Texture,i.uv.xy*_Texture_ST.xy+_Texture_ST.zw+noise*_Texture_noise_int+_RecoveredEffectTime*_Texture_uv)*_Texture_Color*i.color;
float rim=pow(1-dot(i.normal,normalize(_WorldSpaceCameraPos-i.world)),_Fresnel_power)*_Fresnel_scale;
return float4(main.rgb*tex2D(_colortex,i.uv.xy*_colortex_ST.xy+_colortex_ST.zw).rgb+rim*_Fresnel_Color.rgb,main.a*_Alpha);'''),
'Effect/Blend':('sampler2D _TextureSample0,_TextureSample3,_Main_tex,_TextureSample1;float4 _Main_tex_ST,_TextureSample3_ST,_TextureSample1_ST,_texcoord_ST,_Main_color;float2 _Vector0,_Vector1;float _Float1;',
'o.uv.xy=v.uv.xy*_texcoord_ST.xy+_texcoord_ST.zw;',
'''float2 uv=i.uv.xy*_Main_tex_ST.xy+_Main_tex_ST.zw;
float2 noise=tex2D(_TextureSample0,uv+_RecoveredEffectTime*_Vector1).rg*_Float1;
float mask=tex2D(_TextureSample3,i.uv.xy*_TextureSample3_ST.xy+_TextureSample3_ST.zw).r;
float4 c=tex2D(_Main_tex,uv+_RecoveredEffectTime*_Vector0+noise*mask)*_Main_color*i.color;
c.a*=tex2D(_TextureSample1,i.uv.xy*_TextureSample1_ST.xy+_TextureSample1_ST.zw).r;return c;'''),
'Effect/Fire Ruan Dissolves Noise':('''sampler2D _NoiseTexture,_MainTex,_DissolvesTexture,_MaskTexture,_HuoDissolutionTexture;
float4 _NoiseTexTiling_xyOffset_zw,_MainTexTiling_xyOffset_zw,_DissolvesTexture1,_MaskTexTiling_xyOffset_zw,_HuoDissolutionTexture_ST,_EdgeColor,_MainColor;
float _NoiseSpendX,_NoiseSpendY,_DissolvesMode,_Dissolves,_Edge,_Hardness,_UVOffset,_MainSpeed_X,_MainSpeed_Y,_Noise,_MaskX,_MaskY,_HuoDissolutionTexturespeedx,_HuoDissolutionTexturespeedy;''',
'''o.uv.zw=v.uv.xy*_NoiseTexTiling_xyOffset_zw.xy+_NoiseTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*float2(_NoiseSpendX,_NoiseSpendY);
float3 d=lerp(float3(_Dissolves,_Edge,_Hardness),v.uv1.xyz,_DissolvesMode);o.thresholds=(2-d.z)*float2(d.x*(1+d.y),d.x*(1+d.y)-d.y);''',
'''float2 uv=i.uv.xy*_MainTexTiling_xyOffset_zw.xy+_MainTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*_UVOffset*float2(_MainSpeed_X,_MainSpeed_Y);
uv+=(tex2D(_NoiseTexture,i.uv.zw).r-.5)*lerp(_Noise,i.custom.w,_DissolvesMode);
float4 main=tex2D(_MainTex,uv)*_MainColor*i.color;
float d=tex2D(_DissolvesTexture,i.uv.xy*_DissolvesTexture1.xy+_DissolvesTexture1.zw).r+1;
float hardness=lerp(_Hardness,i.custom.z,_DissolvesMode);float2 f=saturate((d-i.thresholds-hardness)/(1-hardness));
float mask=tex2D(_MaskTexture,i.uv.xy*_MaskTexTiling_xyOffset_zw.xy+_MaskTexTiling_xyOffset_zw.zw-_RecoveredEffectTime*float2(_MaskX,_MaskY)).r;
float fire=tex2D(_HuoDissolutionTexture,i.uv.xy*_HuoDissolutionTexture_ST.xy+_HuoDissolutionTexture_ST.zw+_RecoveredEffectTime*float2(_HuoDissolutionTexturespeedx,_HuoDissolutionTexturespeedy)).r;
return float4(lerp(_EdgeColor.rgb,main.rgb,f.x),step(i.uv.y,fire)*saturate(f.y*main.a*mask));''')}
def state(v,values):return '['+v['name']+']' if v['name']!='<noninit>' else values[int(v['val'])]
rows=[]
for info in read(S/'embedded-shader-evidence/manifest.json'):
 t=read(R/info['tree']);form=t['m_ParsedForm'];name=info['name'];props=[]
 for p in form['m_PropInfo']['m_Props']:
  d=[p[f'm_DefValue[{i}]'] for i in range(4)];typ=p['m_Type'];definition={0:'Color',1:'Vector',2:'Float',3:f'Range({d[1]},{d[2]})',4:'2D'}[typ]
  value='('+','.join(str(x) for x in d)+')' if typ in (0,1) else json.dumps(p['m_DefTexture']['m_DefaultName'])+' {}' if typ==4 else str(d[0]);attrs=('[HDR]' if p['m_Flags']&16 else '')+('[HideInInspector]' if p['m_Flags']&1 else '')
  props.append(f'{attrs}{p["m_Name"]}({json.dumps(p["m_Description"],ensure_ascii=False)},{definition})={value}')
 uniforms,vert,frag=forms[name];passes=[]
 for index,pa in enumerate(form['m_SubShaders'][0]['m_Passes']):
  st=pa['m_State'];tags=dict(st['m_Tags']['tags']);blend=st['rtBlend0'];assert st['zTest']['val']==4
  fixed=f"Blend {state(blend['srcBlend'],{0:'Zero',1:'One',5:'SrcAlpha'})} {state(blend['destBlend'],{0:'Zero',1:'One',10:'OneMinusSrcAlpha'})} ZWrite {state(st['zWrite'],{0:'Off',1:'On'})} Cull {state(st['culling'],{0:'Off',1:'Front',2:'Back'})} ZTest LEqual"
  body=frag if index==0 else 'float4 result=sourceColor(i);return float4(0,0,0,result.a);'
  program=common+uniforms+'\n'+vertex.replace('VERTEX',vert)+'float4 sourceColor(v2f i){'+frag+'}\nfloat4 frag(v2f i):SV_Target{'+body+'}\n'
  passes.append('Pass { Name "'+st['m_Name']+'" Tags { "LightMode"="'+('ForwardBase' if index==0 else 'ForwardAdd')+'" } '+fixed+'\nCGPROGRAM\n#pragma vertex vert\n#pragma fragment frag\n#pragma target 3.0\n'+program+'ENDCG\n}')
 tags=' '.join(json.dumps(k)+'='+json.dumps(v) for k,v in form['m_SubShaders'][0]['m_Tags']['tags']);dest=O/(name.replace('/','_').replace(' ','_')+'.shader');dest.write_text('// Original compiled GLES and fixed state: '+info['tree']+'\nShader "AreaBattle/Recovered '+name+'" { Properties {\n'+'\n'.join(props)+'\n} SubShader { Tags { '+tags+' }\n'+'\n'.join(passes)+'\n} }',encoding='utf8')
 rows.append({'sourceName':name,'restoredName':'AreaBattle/Recovered '+name,'path':dest.relative_to(R).as_posix(),'source':info})
m=read(S/'prepared-embedded/native-import.json');m['shaderMap']=rows;(S/'prepared-embedded/native-import.json').write_text(json.dumps(m,ensure_ascii=False,indent=2),encoding='utf8');print('Prepared',len(rows),'original formula shaders')

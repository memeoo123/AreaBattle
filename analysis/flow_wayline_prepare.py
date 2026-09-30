"""Source-only extraction of ordinary WayLine rendering; does not execute original code."""
import json,hashlib,struct
from pathlib import Path
G=Path('analysis/targets/wxcf1394487200e48f/43/generated'); A=G/'unity-assets'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def source(p):return {'path':str(p.relative_to(G.parent)).replace('\\','/'),'sha256':sha(p)}
data={'width':.1,'moveSpeed':-.1,'tilingPerUnit':16,'height':.02,'arrowHeight':.001,'materials':[],'prefabs':[],'textures':[]}
camps=read(G/'tables/CampConfig.json')['Datas'];data['campColors']=[{'r':0,'g':1,'b':0,'a':1} for _ in range(max(c['id'] for c in camps)+1)]
for c in camps:
 h=c['LineColor'].lstrip('#');data['campColors'][c['id']]={k:int(h[j:j+2],16)/255 for k,j in [('r',0),('g',2),('b',4)]}|{'a':1}
for mid in [-3260609361146548237,2133389759313279779]:
 p=next((A/'Material').glob('*__'+str(mid)+'.json'));m=read(p);sp=m['m_SavedProperties'];tex=dict(sp['m_TexEnvs'])['_Tex'];tid=tex['m_Texture']['m_PathID'];tp=next((A/'Texture2D').glob('*__'+str(tid)+'.texture.json'));t=read(tp);png=tp.with_name(tp.name.replace('.texture.json','.png'))
 data['textures'].append({'id':str(tid),'name':t['m_Name'],'path':str(png.relative_to(G.parent)).replace('\\','/'),'sha256':sha(png),'width':t['m_Width'],'height':t['m_Height'],'mipmap':t['m_MipCount']>1,'srgb':t['m_ColorSpace']==1,'settings':t['m_TextureSettings']})
 f=dict(sp['m_Floats']);data['materials'].append({'id':str(mid),'name':m['m_Name'],'textureId':str(tid),'scale':tex['m_Scale'],'offset':tex['m_Offset'],'Vspeed':f['_Vspeed'],'Uspeed':f['_Uspeed'],'opacity':f['_opacity'],'color':dict(sp['m_Colors'])['_color'],'source':source(p)})
for name in ['LineRander_single','LineRander_double']:
 p=A/'prefabs'/f'{name}.hierarchy.json';root=read(p)['root'];renderers=[]
 for n in root['children']:
  c=next(c for c in n['components'] if c['type']=='LineRenderer');d=c['data'];par=d['m_Parameters'];tr=next(c['data'] for c in n['components'] if c['type']=='Transform')
  renderers.append({'name':n['name'],'active':n['active'],'layer':n['layer'],'sourceId':c['id'],'transform':tr,'enabled':d['m_Enabled'],'materialId':str(d['m_Materials'][0]['m_PathID']),'sortingOrder':d['m_SortingOrder'],'sortingLayerID':d['m_SortingLayerID'],'castShadows':d['m_CastShadows'],'receiveShadows':bool(d['m_ReceiveShadows']),'positions':d['m_Positions'],'worldSpace':d['m_UseWorldSpace'],'loop':d['m_Loop'],'parameters':par})
 data['prefabs'].append({'name':name,'source':source(p),'sourceId':root['id'],'layer':root['layer'],'renderers':renderers})
shader=A/'ShaderEvidence/Shader_Forge_Effect_miaozhun.platform0.segment0.strings.txt';st=A/'ShaderEvidence/Shader_Forge_Effect_miaozhun.typetree.json';data['shaderSources']=[source(shader),source(st)]
(G/'wayline-runtime-data.json').write_text(json.dumps(data,indent=2),encoding='utf-8')
e={'target':'wxcf1394487200e48f/43','status':'static-confirmed-ready-for-import-validation','rules':[
 {'id':'wayline-runtime-width','fact':'Config113 Content1 splits to width=.1, moveSpeed=-.1*initial gameTimeScale (config101=1), tilingPerUnit=16. Init calls LineRenderer.startWidth only on each source one-key curve.','sources':['wasmcode f12780 @005deec3..005df02c','f2941 @000de2ae..000de33a','GlobalValueConfig id113']},
 {'id':'wayline-direction-prefab','fact':'direction0 hides line; direction2 reverses small/large endpoints. direction3 AND different camps selects double prefab11; otherwise single10. Colors are LevelControl.GetCampColor(from/to). Collider enabled iff either camp is player.','sources':['f9366 @00456d68..00456db0','f9387 @004590e0..00459108,00459161..004591a7,0045922d..0045940c','f6783 @0027138c..00271395']},
 {'id':'wayline-camp-palette','fact':'CampConfig.LineColor is parsed from #+hex by ConfigMgr f12781 and cached in dictionary272. LevelControl.GetCampColor forwards ConfigMgr.GetCampColor, fallback Color.green for absent camp.','sources':['f12781 @005df209..005df25b','f4385 @00163b8b..00163b96','f8087 @003a96e1..003a976a','generated/tables/CampConfig.json']},
 {'id':'wayline-geometry','fact':'Endpoints get world up*.02. Arrow segments add another up*.001. Single is from→to; double uses from→midpoint and to→midpoint. Both complete segments use their origin camp color.','sources':['f7618 @0037857d..00378645,0037864a..00378e40']},
 {'id':'wayline-modes','fact':'SetLine args5 enableCollider, args6 showArrow. Both bg renderers enabled=!showArrow; arrow1 enabled=showArrow. Double arrow2 is not changed and retains prefab enabled=true. Ordinary GetLineObj passes showArrow=true. Single preview should use showArrow=false.','sources':['f7618 @003788bf..003788cb,00378e21..00378e56','f9387 @00459406..0045940c']},
 {'id':'wayline-material','fact':'Each segment sets _Tex scale=(distance*16,1), MPB _Vspeed=-.1 and _color=campColor. MPB is shared and retains both fields.','sources':['f7616 @00378124..003782d3','f2581 @000c6e38..000c6ef5','f2941 @000de2d0..000de33a']},
 {'id':'wayline-fragment','fact':'Time is signed fmod(_Time.y,200000); UV=(uv+time*(_Vspeed,_Uspeed))*_Tex_ST.xy+offset; output rgb=texture.rgb*_color.rgb; alpha=clamp(texture.a*_opacity,0,1), color alpha and vertex color ignored. Blend SrcAlpha OneMinusSrcAlpha, ZWriteOff, CullOff, LEqual. Material opacity1, Uspeed0.','sources':data['shaderSources']},
 {'id':'wayline-collider','fact':'Enabled collider at endpoint midpoint (already y+.02), root LookAt(to), BoxCollider size(.1,.1,length). No endpoint trimming.','sources':['f7618 @00378e59..00378fc6']}
 ],'dependencies':data['textures']+data['shaderSources'],'goldenCases':[
 {'id':'line-unit-single','input':{'from':[0,0,0],'to':[1,0,0],'direction':1},'expected':{'arrowPositions':[[0,.021,0],[1,.021,0]],'textureScale':[16,1],'startWidth':.1,'bgEnabled':False}},
 {'id':'line-double-four-units','input':{'from':[0,0,0],'to':[0,0,4],'direction':3,'differentCamps':True},'expected':{'arrow1':[[0,.021,0],[0,.021,2]],'arrow2':[[0,.021,4],[0,.021,2]],'eachTextureScale':[32,1]}},
 {'id':'line-direction2','input':{'small':[0,0,0],'large':[1,0,0],'direction':2},'expected':{'arrowPositions':[[1,.021,0],[0,.021,0]]}},
 {'id':'line-shader-time','input':{'uv':[.5,.25],'time':2,'speed':[-.1,0],'tiling':[16,1]},'expected':{'uv':[4.8,.25]}},
 {'id':'line-collider','input':{'from':[0,0,0],'to':[0,0,4],'playerInvolved':True},'expected':{'position':[0,.02,2],'size':[.1,.1,4]}},
 ],'unknowns':['Original matched-frame rendering remains unvalidated. Shader is source-equivalent GLSL→HLSL translation, not original source file.','Cut trail prefab is inventoried separately; this scope does not synthesize a cut effect.','Double preview arrow2 unchanged behavior preserved, although known drag preview uses single.']}
(G/'wayline-runtime-evidence.json').write_text(json.dumps(e,indent=2),encoding='utf-8')
print('Prepared',len(data['prefabs']),'prefabs',len(data['textures']),'textures',len(e['rules']),'rules')

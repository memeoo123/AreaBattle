"""Use the native gesture serializer for the two original skill target indicators."""
from pathlib import Path
s=Path('analysis/flow_gesture_prepare.py').read_text(encoding='utf8')
s=s.replace("O=G/'gesture-visuals'","O=G/'skill-target-visuals'")
s=s.replace("G/'resource-snapshots/arrow-projectile-20260928/asset-evidence-incremental.json'","G/'asset-evidence.json'")
s=s.replace("['LineArrow','LineRanderCut']","['LineArrow_hero_blue','LineArrow_hero_red']")
start=s.index("f=read(G/'resource-snapshots/arrow-projectile")
end=s.index('for name,ids in hierarchies:',start)
s=s[:start]+s[end:]
s=s.replace('assert pscount==6','assert pscount==2')
s=s.replace("'gesture-visual-runtime.json'","'skill-target-runtime.json'")
s=s[:s.index('for ident in sorted(external):')]
exec(compile(s,'skill target native prepare','exec'))
for ident in sorted(external):
 o=objects[ident];assert o['type']=='Material'
 manifest['externalAssets'].append({'id':ident,'placeholder':guid(ident),'name':o['name'],'type':'Material'})
 d=reader(ident).read_typetree();saved=d['m_SavedProperties'];sid=next(x['pathId']for x in o['references']if x['property']=='.m_Shader');assert sid==203
 tv=dict(saved['m_TexEnvs'])['_MainTex'];ptr=tv['m_Texture'];assert ptr['m_FileID']==0
 tid=o['serializedFile']+':'+str(ptr['m_PathID']);t=objects[tid];assert t['type']=='Texture2D'
 td=read(R/t['outputs']['textureRaw'])
 if not any(x['id']==tid for x in manifest['textures']):manifest['textures'].append({'id':tid,'name':t['name'],'path':t['outputs']['png'],'sha256':sha(R/t['outputs']['png']),'width':td['m_Width'],'height':td['m_Height'],'mipmap':td['m_MipCount']>1,'srgb':td['m_ColorSpace']==1,'settings':td['m_TextureSettings']})
 manifest['materials'].append({'id':ident,'name':o['name'],'textureId':tid,'shader':'Legacy Shaders/Particles/Alpha Blended','renderQueue':d['m_CustomRenderQueue'],'lightmapFlags':d['m_LightmapFlags'],'instancing':d['m_EnableInstancingVariants'],'doubleSidedGI':d['m_DoubleSidedGI'],'scale':tv['m_Scale'],'offset':tv['m_Offset'],'floats':[{'name':k,'value':v}for k,v in saved['m_Floats']],'colors':[{'name':k,'value':v}for k,v in saved['m_Colors']],'source':o['outputs']['typetree'],'sha256':sha(R/o['outputs']['typetree'])})
assert pscount==2
(G/'skill-target-runtime.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
print(json.dumps({'prefabs':len(manifest['prefabs']),'particleSystems':pscount,'materials':len(manifest['materials'])}))

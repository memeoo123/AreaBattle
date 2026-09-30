"""Native particle/trail prefabs from original type trees; no original scripts execute."""
import json,sys,hashlib
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from native_unity_serialization import native_tree,document
R=Path('analysis/targets/wxcf1394487200e48f/43');G=R/'generated';O=G/'gesture-visuals';O.mkdir(exist_ok=True)
def read(p):return json.loads(p.read_text(encoding='utf8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
objects={o['id']:o for o in read(G/'resource-snapshots/arrow-projectile-20260928/asset-evidence-incremental.json')['objects']}
cache={}
def reader(ident):
 o=objects[ident];s=o['source']
 if s not in cache:cache[s]={str(x.assets_file.name)+':'+str(x.path_id):x for x in UnityPy.load(str(R/s)).objects}
 return cache[s][ident]
def guid(ident):return hashlib.md5(('gesture:'+ident).encode()).hexdigest()
manifest={'prefabs':[],'materials':[],'textures':[],'meshes':[],'externalAssets':[]}
external=set();pscount=0
hierarchies=[]
for name in ['LineArrow','LineRanderCut']:
 f=read(O/(name+'.hierarchy.json'));ids=[]
 def walk(n):
  ids.append(n['id']);ids.extend(c['id']for c in n['components'])
  for c in n['children']:walk(c)
 walk(f['root']);hierarchies.append((name,ids))
f=read(G/'resource-snapshots/arrow-projectile-20260928/prefabs/hdzd_eff_spheretrails.flat.json')
hierarchies.append(('hdzd_eff_SphereTrails',[x for n in f['nodes'] for x in [n['object']]+[c['id']for c in n['components']]]))
for name,ids in hierarchies:
 local={ident:1000000000000+i for i,ident in enumerate(ids)};documents=[];records=[]
 for ident in ids:
  o=objects[ident];r=reader(ident);data=r.read_typetree()
  def pointer(v,path):
   if not v['m_PathID']:return {'fileID':0}
   fid=v['m_FileID'];sf=o['serializedFile'] if fid==0 else r.assets_file.externals[fid-1].path.split('/')[-1]
   target=sf.replace('unity default resources','unity_default_resources')+':'+str(v['m_PathID'])
   if target in local:return {'fileID':local[target]}
   assert target in objects,(ident,path,target)
   typ=objects[target]['type'];assert typ in ['Material','Mesh'],(ident,path,typ)
   external.add(target);return {'fileID':2100000 if typ=='Material' else 4300000,'guid':guid(target),'type':2}
  value=native_tree(r.serialized_type.node,data,pointer)
  # Original player GameObject tags are preserved in evidence, but these three prefabs are Untagged.
  assert r.type.name in ['GameObject','Transform','RectTransform','ParticleSystem','ParticleSystemRenderer','TrailRenderer']
  documents.append(document(r.type.value,local[ident],r.type.name,value))
  records.append({'id':ident,'type':o['type'],'fileId':local[ident],'source':o['source'],'sourceSha256':sha(R/o['source'])})
  if o['type']=='ParticleSystem':pscount+=1
 p=O/(name+'.prefab.template');p.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+''.join(documents),encoding='utf8')
 manifest['prefabs'].append({'name':name,'template':str(p.relative_to(R)).replace('\\','/'),'sha256':sha(p),'components':records})
for ident in sorted(external):
 o=objects[ident];manifest['externalAssets'].append({'id':ident,'placeholder':guid(ident),'name':o['name'],'type':o['type']})
 if o['type']=='Mesh':
  d=read(R/o['outputs']['meshNative']);m={'id':ident,'name':o['name'],'source':o['outputs']['meshNative'],'sha256':sha(R/o['outputs']['meshNative'])}
  for k,axes in [('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')]:m[k]=[{a:v for a,v in zip(axes,row)}for row in d.get(k)or[]]
  m['submeshes']=[{'indices':[i for tri in ts for i in tri]}for ts in d['submeshTriangles']];m['bounds']=reader(ident).read_typetree()['m_LocalAABB'];manifest['meshes'].append(m)
 else:
  d=reader(ident).read_typetree();saved=d['m_SavedProperties'];tex=next(x for x in o['references']if 'm_TexEnvs' in x['property']);tid=tex['target'];t=objects[tid];td=read(R/t['outputs']['textureRaw'])
  tv=dict(saved['m_TexEnvs'])['_MainTex'];sid=next(x['pathId']for x in o['references']if x['property']=='.m_Shader');assert sid in [200,203]
  manifest['materials'].append({'id':ident,'name':o['name'],'textureId':tid,'shader':'Legacy Shaders/Particles/'+('Additive'if sid==200 else'Alpha Blended'),'renderQueue':d['m_CustomRenderQueue'],'lightmapFlags':d['m_LightmapFlags'],'instancing':d['m_EnableInstancingVariants'],'doubleSidedGI':d['m_DoubleSidedGI'],'scale':tv['m_Scale'],'offset':tv['m_Offset'],'floats':[{'name':k,'value':v}for k,v in saved['m_Floats']],'colors':[{'name':k,'value':v}for k,v in saved['m_Colors']],'source':o['outputs']['typetree'],'sha256':sha(R/o['outputs']['typetree'])})
  manifest['textures'].append({'id':tid,'name':t['name'],'path':t['outputs']['png'],'sha256':sha(R/t['outputs']['png']),'width':td['m_Width'],'height':td['m_Height'],'mipmap':td['m_MipCount']>1,'srgb':td['m_ColorSpace']==1,'settings':td['m_TextureSettings']})
assert pscount==6
(G/'gesture-visual-runtime.json').write_text(json.dumps(manifest,indent=2),encoding='utf8')
print(json.dumps({'prefabs':len(manifest['prefabs']),'particleSystems':pscount,'materials':len(manifest['materials']),'textures':len(manifest['textures']),'meshes':len(manifest['meshes'])}))

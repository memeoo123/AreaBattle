"""Prepare native geometry, original renderer fields and zero-collider visual prefabs."""
import json,hashlib
from pathlib import Path
R=Path('analysis/targets/wxcf1394487200e48f/43');G=R/'generated';S=G/'resource-snapshots/obstacle-visuals-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def ref(c,prop):return next(x['target'] for x in c['references'] if x['property']==prop)
def resource(o):return {'id':o['id'],'name':o['name'],'sourceBundle':o['source'],'files':[{'kind':k,'path':p,'sha256':sha(R/p)} for k,p in o['outputs'].items()]}
objects={o['id']:o for o in read(G/'asset-evidence.json')['objects']}
objects.update({o['id']:o for o in read(S/'asset-evidence-incremental.json')['objects']})
prefabs=[];meshes=set();materials=set()
for x in read(R/'evidence/obstacle-downloads.json'):
 p=S/'prefabs'/(x['name'].lower()+'.flat.json');f=read(p);nodes=[];indices={n['path']:i for i,n in enumerate(f['nodes'])}
 for n in f['nodes']:
  d={'name':n['name'],'sourceId':n['object'],'active':n['active'],'layer':n['layer'],'parent':indices[n['parent']] if n['parent'] else -1,'transform':n['transform'],'meshId':'','materialIds':[],'renderer':None}
  mf=next((c for c in n['components'] if c['type']=='MeshFilter'),None);mr=next((c for c in n['components'] if c['type']=='MeshRenderer'),None)
  assert bool(mf)==bool(mr)
  if mf:
   d['meshId']=ref(mf,'.m_Mesh');meshes.add(d['meshId']);d['renderer']=mr['data'];d['materialIds']=[r['target'] for r in mr['references'] if r['property'].startswith('.m_Materials[')];materials.update(d['materialIds'])
  nodes.append(d)
 prefabs.append({'entityId':x['entityId'],'name':x['name'],'sourcePath':str(p.relative_to(R)).replace('\\','/'),'sourceSha256':sha(p),'sourceBundle':x['path'],'sourceBundleSha256':x['sha256'],'nodes':nodes})
assert len(meshes)==len(materials)==1
meshObj=objects[next(iter(meshes))];nativePath=R/meshObj['outputs']['meshNative'];native=read(nativePath)
mesh={'id':meshObj['id'],'name':meshObj['name'],'source':resource(meshObj),'coordinateSpace':native['coordinateSpace']}
mesh['bounds']=read(S/'mesh-original-bounds.json')['localBounds']
for key,axes in [('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')]:
 values=native.get(key);mesh[key]=[{a:v for a,v in zip(axes,row)}for row in values] if values else [];mesh['has'+key[0].upper()+key[1:]]=bool(values)
mesh['submeshes']=[{'indices':[i for tri in ts for i in tri]}for ts in native['submeshTriangles']]
assert not native.get('boneIndices') and not native.get('boneWeights') and not native.get('bindPose')
matObj=objects[next(iter(materials))];mat=read(R/matObj['outputs']['typetree']);texRef=next(r for r in matObj['references'] if r['property'].startswith('.m_SavedProperties.m_TexEnvs') and r['target']);texObj=objects[texRef['target']];tex=read(R/texObj['outputs']['textureRaw'])
mt=dict(mat['m_SavedProperties']['m_TexEnvs'])['_MainTex']
material={'id':matObj['id'],'name':mat['m_Name'],'source':resource(matObj),'shader':'Unlit/Texture','shaderSourceId':'unity_builtin_extra:10752','shaderEvidence':str((S/'shaders/builtin10752.json').relative_to(R)).replace('\\','/'),'renderQueue':mat['m_CustomRenderQueue'],'scale':mt['m_Scale'],'offset':mt['m_Offset'],'textureId':texObj['id']}
texture={'id':texObj['id'],'name':tex['m_Name'],'source':resource(texObj),'path':texObj['outputs']['png'],'sha256':sha(R/texObj['outputs']['png']),'width':tex['m_Width'],'height':tex['m_Height'],'mipmap':tex['m_MipCount']>1,'srgb':tex['m_ColorSpace']==1,'settings':tex['m_TextureSettings']}
descriptor={'prefabs':prefabs,'meshes':[mesh],'materials':[material],'textures':[texture],'attachment':'Instantiate visual prefab under existing corresponding collision root; overwrite visual ROOT localPosition=0/localRotation=identity/localScale=1 because collision root already receives ObstacleInfoCfg.SetTransform. Preserve every descendant source transform. Do not add colliders.'}
(G/'obstacle-visual-runtime.json').write_text(json.dumps(descriptor,indent=2),encoding='utf-8')
rules={'target':'wxcf1394487200e48f/43','sourceSnapshot':str(S.relative_to(R)).replace('\\','/'),'coverage':{'entityIds':[p['entityId']for p in prefabs],'prefabCount':len(prefabs),'rendererCount':sum(bool(n['meshId'])for p in prefabs for n in p['nodes']),'uniqueMeshes':len(meshes),'uniqueMaterials':len(materials),'uniqueTextures':1,'unresolvedReferences':0,'sourceColliders':125,'importedVisualColliders':0},'sourcePreservation':read(S/'unity-consumption-index.json')['baselinePreserved'],'rules':[
 {'id':'obstacle-native-mesh','status':'confirmed','fact':'All15 used prefabs reuse HD3_wall native geometry; no UnityPy OBJ X mirror or winding reversal is applied.','source':resource(meshObj)},
 {'id':'obstacle-opaque-unlit','status':'confirmed','fact':'Every used renderer references wall material→unity_builtin_extra shader10752, whose original parsed name is Unlit/Texture. GLES2 fragment returns sampled MainTex.rgb and alpha1. Render state opaque, Src=One,Dst=Zero,Cull=Back,ZWrite=On. Material texture scale1,offset0.','source':material},
 {'id':'obstacle-source-transforms','status':'confirmed','fact':'277 hierarchy nodes and125 renderer/collider nodes retain original local positions, quaternions, scales, active flags and layer. Runtime replaces original root transform with level cfg/100; visual child root identity prevents applying prefab scale twice.','source':'generated/obstacle-evidence.json:transform SetTransform f11209'},
 {'id':'visual-only-attachment','status':'implementation-adapter','fact':'Prefab contains MeshFilter/MeshRenderer only. Attach beside existing checked collision data by nesting under its root; existing125colliders/physics topology are unchanged. No debug collider cubes or new collision meshes.'}
 ],'unknowns':['Original matched-frame visual acceptance remains pending. Built-in shader uses the current Unity Unlit/Texture implementation; original GLES/name/state retained for comparison.'],
 'goldens':{'meshVertices':len(mesh['vertices']),'meshIndices':sum(len(s['indices'])for s in mesh['submeshes']),'subMeshCount':len(mesh['submeshes']),'textureSize':[tex['m_Width'],tex['m_Height']],'prefabRenderers':{str(p['entityId']):sum(bool(n['meshId'])for n in p['nodes'])for p in prefabs},'rootAttachTransform':{'position':[0,0,0],'rotation':[0,0,0,1],'scale':[1,1,1]}}}
(G/'obstacle-visual-evidence.json').write_text(json.dumps(rules,indent=2),encoding='utf-8')
print(json.dumps(rules['coverage']));print(json.dumps(rules['goldens']))

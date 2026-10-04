"""Prepare fully resolved native component YAML and editable dependency descriptors."""
import json,sys,hashlib,collections
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from native_unity_serialization import native_tree,document
_source_document=document
def document(*args):
 # Match this installed Unity's own .anim serialization; YAML .inf imports as NaN.
 return _source_document(*args).replace(': .inf\n',': Infinity\n').replace(': -.inf\n',': -Infinity\n').replace(': .nan\n',': NaN\n')
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';S=R/'generated/resource-snapshots/task-box-effect-20261003';O=S/'prepared';O.mkdir(exist_ok=True)
read=lambda p:json.loads(p.read_text(encoding='utf8'))
report=read(S/'asset-evidence-incremental.json');objects={o['id']:o for o in report['objects']};deps={d['serializedFile']:d for d in report['dependencies']};base=read(R/'generated/asset-evidence.json')
for o in base['objects']:objects.setdefault(o['id'],o)
for d in base['dependencies']:deps.setdefault(d['serializedFile'],d)
environments={};resources={};prefabs=[];skipped=[]
def asset_tree(node,value,resolve_pointer,path=''):
 if node.m_Type.startswith('PPtr<'):return resolve_pointer(value,path)
 if node.m_Type=='map':
  array=node.m_Children[0];pair=next(c for c in array.m_Children if c.m_Name=='data');second=next(c for c in pair.m_Children if c.m_Name=='second')
  return [{str(k):asset_tree(second,v,resolve_pointer,path+'.'+str(k))} for k,v in value]
 if isinstance(value,dict):
  result={'serializedVersion':node.m_Version} if node.m_Version>1 else {};children={c.m_Name:c for c in node.m_Children}
  for k,v in value.items():result[k]=asset_tree(children[k],v,resolve_pointer,path+'.'+k)
  return result
 if isinstance(value,(list,tuple)):
  array=next((c for c in node.m_Children if c.m_Type=='Array'),node);data=next(c for c in array.m_Children if c.m_Name=='data')
  return [asset_tree(data,v,resolve_pointer,path+f'[{i}]') for i,v in enumerate(value)]
 return value
def reader(o):
 if o['source'] not in environments:environments[o['source']]=UnityPy.load(str(R/o['source']))
 return next(x for x in environments[o['source']].objects if x.path_id==o['pathId'] and x.assets_file.name==o['serializedFile'])
def tree(o):return read(R/o['outputs']['typetree']) if o['outputs'].get('typetree') else reader(o).read_typetree()
def resolve(o,p):
 if not p['m_PathID']:return None
 cab=o['serializedFile']
 if p['m_FileID']:cab=deps[cab]['externals'][p['m_FileID']-1]['path'].rsplit('/',1)[-1]
 if cab=='unity default resources':cab='unity_default_resources'
 return cab+':'+str(p['m_PathID'])
def resource(ident):
 if ident in resources:return resources[ident]
 o=objects[ident];index=len(resources);row={'id':ident,'index':index,'type':o['type'],'name':o['name'],'source':o['source'],'sourcePathId':str(o['pathId'])};resources[ident]=row
 if o['serializedFile'] in ('unity_builtin_extra','unity_default_resources'):
  row.update(builtin=True,guid='0000000000000000'+('f' if o['serializedFile']=='unity_builtin_extra' else 'e')+'000000000000000',fileId=str(o['pathId']));return row
 if o['type'] in ('Texture2D','Sprite'):
  t=tree(o);path=o['outputs'].get('pngCanvas',o['outputs'].get('png'));assert path,ident
  row.update(path=path,sha256=hashlib.sha256((R/path).read_bytes()).hexdigest())
  if o['type']=='Sprite':row.update(pivot=t['m_Pivot'],ppu=t['m_PixelsToUnits'],border=t['m_Border'])
  else:row.update(sRGB=t['m_ColorSpace']==1,mipCount=t['m_MipCount'],settings=t['m_TextureSettings'])
 elif o['type']=='Material':
  t=tree(o);shader=objects[resolve(o,t['m_Shader'])];sh=resource(shader['id']);row.update(shaderIndex=sh['index'],renderQueue=t['m_CustomRenderQueue'],keywords=t.get('m_ShaderKeywords',' '.join(t.get('m_ValidKeywords',[]))),enableInstancing=bool(t.get('m_EnableInstancingVariants')),doubleSidedGI=bool(t.get('m_DoubleSidedGI')),lightmapFlags=t.get('m_LightmapFlags',4),tags=[{'name':k,'value':v} for k,v in t.get('stringTagMap',[])],disabledPasses=t.get('disabledShaderPasses',[]))
  saved=t['m_SavedProperties'];row['floats']=[{'name':k,'value':v} for k,v in saved['m_Floats']];row['colors']=[{'name':k,'value':v} for k,v in saved['m_Colors']];row['textures']=[]
  for name,e in saved['m_TexEnvs']:
   tex=resolve(o,e['m_Texture']);row['textures'].append({'name':name,'resourceIndex':resource(tex)['index'] if tex else -1,'scale':e['m_Scale'],'offset':e['m_Offset']})
  rr=reader(o)
  def ptr(p,path):
   ident=resolve(o,p)
   return {'fileID':0} if ident is None else external(ident)
  native=asset_tree(rr.serialized_type.node,rr.read_typetree(),ptr);native['m_ObjectHideFlags']=0
  dest=O/f'material-{index}.mat.template';dest.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+document(21,2100000,'Material',native),encoding='utf8');row['templatePath']=str(dest.relative_to(R)).replace('\\','/')
 elif o['type']=='Shader':
  row['shaderName']=o['name'];row['restoredShaderName']='AreaBattle/Recovered '+o['name'] if o['name'].startswith('Effect/') else o['name']
 elif o['type']=='Mesh':
  path=o['outputs']['meshNative'];m=read(R/path);row['sourceMesh']=path;row['sourceSha256']=hashlib.sha256((R/path).read_bytes()).hexdigest()
  for key,names in [('vertices','xyz'),('normals','xyz'),('uv0','xy'),('uv1','xy'),('colors','rgba'),('tangents','xyzw')]:row[key]=[dict(zip(names,v)) for v in m[key]] if m.get(key) else []
  row['submeshes']=[{'indices':[i for tri in sub for i in tri]} for sub in m['submeshTriangles']]
  row['boneWeights']=[{**{f'boneIndex{i}':int(a[i]) for i in range(4)},**{f'weight{i}':b[i] for i in range(4)}} for a,b in zip(m.get('boneIndices')or[],m.get('boneWeights')or[])]
  row['bindPoses']=[{'m'+k[1:]:v for k,v in mat.items()} for mat in m.get('bindPose')or[]]
 elif o['type']=='AnimationClip':
  rr=reader(o);t=rr.read_typetree();assert t['m_Legacy'] and not t['m_CompressedRotationCurves'],('Unsupported animation',ident)
  def ptr(p,path):
   ident=resolve(o,p)
   if ident is None:return {'fileID':0}
   return external(ident)
  native=native_tree(rr.serialized_type.node,t,ptr);native['m_ObjectHideFlags']=0
  p=O/f'animation-{index}.anim.template';p.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+document(74,7400000,'AnimationClip',native),encoding='utf8');row['templatePath']=str(p.relative_to(R)).replace('\\','/');row['duration']=max((k['time'] for family in ('m_RotationCurves','m_PositionCurves','m_ScaleCurves','m_FloatCurves') for c in t[family] for k in c['curve']['m_Curve']),default=0)
 else:raise ValueError('Unsupported referenced resource '+ident+' '+o['type'])
 return row
def external(ident):
 row=resource(ident)
 if row.get('builtin'):return {'fileID':int(row['fileId']),'guid':row['guid'],'type':0}
 return {'fileID':'__FILEID_'+str(row['index'])+'__','guid':'__GUID_'+str(row['index'])+'__','type':3 if row['type'] in ('Shader','Texture2D','Sprite') else 2}
allowed={'GameObject','RectTransform','Transform','ParticleSystem','ParticleSystemRenderer','TrailRenderer','MeshFilter','MeshRenderer','SkinnedMeshRenderer','SpriteRenderer','Animation'}
for p in read(S/'unity-consumption-index.json')['prefabs']:
 flat=read(R/p['flatData']);ids=[];nodePaths={}
 for n in flat['nodes']:
  ids.append(n['object']);nodePaths[n['object']]=n['path']
  for c in n['components']:
   if c['type']=='MonoBehaviour':skipped.append({'prefab':p['name'],'id':c['id'],'script':c['script']['m_ClassName'],'reason':'Original executable MonoBehaviour excluded; recovered mechanics own behavior'});continue
   assert c['type'] in allowed,c['type'];ids.append(c['id']);nodePaths[c['id']]=n['path']
 local={ident:1000000000001+i for i,ident in enumerate(ids)};docs=[];components=[]
 for ident in ids:
  o=objects[ident];rr=reader(o);t=rr.read_typetree()
  if o['type']=='GameObject':
   t['m_Component']=[c for c in t['m_Component'] if resolve(o,c['component']) in local]
  def ptr(value,path,owner=o):
   ident=resolve(owner,value)
   if ident is None:return {'fileID':0}
   if ident in local:return {'fileID':local[ident]}
   return external(ident)
  native=native_tree(rr.serialized_type.node,t,ptr);native['m_ObjectHideFlags']=0
  if o['type']=='GameObject':
   assert native.pop('m_Tag')==0;native['m_TagString']='Untagged';native['m_Icon']={'fileID':0};native['m_NavMeshLayer']=0;native['m_StaticEditorFlags']=0
  docs.append(document(rr.class_id,local[ident],o['type'],native));components.append({'id':ident,'fileId':str(local[ident]),'type':o['type'],'path':nodePaths[ident][len(flat['nodes'][0]['path']):].lstrip('/')})
 dest=O/(p['name']+'.prefab.template');dest.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+''.join(docs),encoding='utf8')
 prefabs.append({'name':p['name'],'originalName':flat['nodes'][0]['name'],'templatePath':str(dest.relative_to(R)).replace('\\','/'),'sourceRoot':flat['rootObject'],'components':components,'sourceComponentCounts':dict(collections.Counter(x['type'] for x in components))})
out={'target':{'appId':'wxcf1394487200e48f','version':'43'},'prefabs':prefabs,'resources':list(resources.values()),'skippedOriginalCode':skipped,'shaderMap':read(O/'shader-map.json'),'unknowns':['Texture assets use decoded base-level source PNG; mipmaps when requested are generated by current Unity. Original compressed platform mip bytes remain in source bundles.'],'validation':{'prefabs':len(prefabs),'resources':dict(collections.Counter(x['type'] for x in resources.values())),'unresolved':0}}
(O/'native-import.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');print(json.dumps(out['validation']))

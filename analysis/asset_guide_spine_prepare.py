"""Preserve original GuideUI YD_0 Spine 4.1.16 subtree and its local dependency closure."""
from pathlib import Path
source=(Path(__file__).parent/'flow_boss_native_prepare.py').read_text(encoding='utf8').split("allowed={'GameObject'")[0]
source=source.replace("boss-entities-20260928","hud-soldier300-20260928").replace("O=S/'prepared'","O=S/'guide-spine/native'").replace("O.mkdir(exist_ok=True)","O.mkdir(parents=True,exist_ok=True)").replace("S/'spine-sources.json'","S/'guide-spine/spine-sources.json'")
exec(compile(source,'guide_spine_shared_native','exec'))
root='CAB-71a96badbcffab7b7451a783f271bc4b:-5489945122626182919';go=objects[root];gt=tree(go);ids=[root]+[resolve(go,x['component']) for x in gt['m_Component']];local={i:1000000000001+k for k,i in enumerate(ids)};docs=[];components=[]
for ident in ids:
 o=objects[ident];rr=reader(o);t=tree(o)
 def ptr(p,path,owner=o):
  i=resolve(owner,p)
  if i is None or owner['type']=='Transform' and path=='.m_Father':return {'fileID':0}
  if i in local:return {'fileID':local[i]}
  return external(i)
 native=(plain_tree(t,ptr) if o['type']=='MonoBehaviour' else native_tree(rr.serialized_type.node,t,ptr));native['m_ObjectHideFlags']=0
 if o['type']=='GameObject':
  assert native.pop('m_Tag')==0;native.update(m_TagString='Untagged',m_Icon={'fileID':0},m_NavMeshLayer=0,m_StaticEditorFlags=0)
 docs.append(document(rr.class_id,local[ident],o['type'],native));components.append({'id':ident,'fileId':str(local[ident]),'type':o['type'],'path':''})
dest=O/'YD_0.prefab.template';dest.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+''.join(docs),encoding='utf8')
out={'prefabs':[{'name':'YD_0','originalName':'YD_0','templatePath':dest.relative_to(R).as_posix(),'sourceRoot':root,'components':components}],'resources':list(resources.values()),'shaderMap':[],'unknowns':[],'spine':list(spineSources.values()),'evidence':{'initialAnimation':'YD','loop':True,'updateTiming':1,'unscaledTime':False,'timeScale':1,'version':'4.1.16','Awake':'GuideUI.Awake f19289 first sets YD_0 inactive; ShowUI stage1 activates it and disables guideIcon Image. Later same-instance ShowUI does not explicitly deactivate it. mainbg visibility controls popup visibility.','transforms':'Original YD_0 position(-1.07,-37.71999,0), localScale(100,100,1), layer5 retained. Its own SkeletonDataAsset scale is preserved.'}}
(O/'native-import.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');print({'resources':dict(collections.Counter(r['type'] for r in resources.values()))})

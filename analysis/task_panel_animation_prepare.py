"""Recover original TaskPanelUI Animation fields and both complete legacy clips."""
import json,hashlib,math
from pathlib import Path
import UnityPy
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43';G=R/'generated'
def read(p):return json.loads(p.read_text(encoding='utf8'))
reports=[read(G/'asset-evidence.json'),read(G/'resource-snapshots/task-panel-ui-20261003/asset-evidence-incremental.json')]
objects={o['id']:o for j in reports for o in j['objects']};deps={d['serializedFile']:d for j in reports for d in j['dependencies']}
panels=[('TaskPanelUI',next(p for p in read(G/'outgame/task-panel-ui-evidence.json')['prefabs']if p['assetPath'].endswith('taskpanelui.prefab')))]
prepared=read(G/'outgame/task-panel-ui-import.json');retained={p['name']:{n['path'] for n in p['nodes']} for p in prepared['prefabs']}
def resolve(owner,ptr):
 if not ptr['m_PathID']:return None
 cab=owner['serializedFile']
 if ptr['m_FileID']:cab=deps[cab]['externals'][ptr['m_FileID']-1]['path'].rsplit('/',1)[-1]
 return cab+':'+str(ptr['m_PathID'])
def script_name(owner,p):
 ident=resolve(owner,p)
 if not ident:return None
 o=objects[ident];return read(R/o['outputs']['typetree']).get('m_ClassName')
def scalar(v,axis):return v[axis] if axis else v
def key(k,axis):
 out={'time':k['time'],'value':scalar(k['value'],axis),'weightedMode':k.get('weightedMode',0)}
 for source,dest in [('inSlope','inTangent'),('outSlope','outTangent'),('inWeight','inWeight'),('outWeight','outWeight')]:
  v=scalar(k.get(source,0),axis)
  out[dest]=v if math.isfinite(v) else 0
  if not math.isfinite(v):out[dest+'Special']='Infinity' if v>0 else '-Infinity' if v<0 else 'NaN'
 return out
clips={};animations=[]
for panel,p in panels:
 root=p['nodes'][0]['path']
 for n in p['nodes']:
  for c in n['components']:
   if c.get('class',c.get('type'))!='Animation':continue
   o=objects[c['id']];env=UnityPy.load(str(R/o['source']));tree=next(x.read_typetree() for x in env.objects if x.path_id==o['pathId'])
   ids=[resolve(o,x) for x in tree['m_Animations']];relative=n['path'][len(root):].lstrip('/')
   animations.append({'panel':panel,'path':relative,'sourceId':o['id'],'sourceBundle':o['source'],'defaultClipId':resolve(o,tree['m_Animation']),'clipIds':ids,'enabled':bool(tree['m_Enabled']),'playAutomatically':tree['m_PlayAutomatically'],'wrapMode':tree['m_WrapMode'],'animatePhysics':tree['m_AnimatePhysics'],'cullingType':tree['m_CullingType'],'retainedNode':relative in retained[panel]})
   for ident in ids:
    if ident in clips:continue
    src=objects[ident];path=R/src['outputs']['typetree'];t=read(path);curves=[]
    for family,prop,axes,kind in [('m_FloatCurves',None,[None],None),('m_PositionCurves','m_LocalPosition',['x','y','z'],'Transform'),('m_ScaleCurves','m_LocalScale',['x','y','z'],'Transform'),('m_EulerCurves','localEulerAnglesRaw',['x','y','z'],'Transform'),('m_RotationCurves','m_LocalRotation',['x','y','z','w'],'Transform')]:
     for source in t.get(family,[]):
      for axis in axes:
       component=kind or (script_name(src,source['script']) if source['classID']==114 else {1:'GameObject',4:'Transform',224:'RectTransform',225:'CanvasGroup'}.get(source['classID'],'ClassID:'+str(source['classID'])))
       curves.append({'path':source['path'],'component':component,'property':prop+'.'+axis if prop else source['attribute'],'preInfinity':source['curve']['m_PreInfinity'],'postInfinity':source['curve']['m_PostInfinity'],'keys':[key(k,axis) for k in source['curve']['m_Curve']]})
    clips[ident]={'sourceId':ident,'name':t['m_Name'],'sourceJson':src['outputs']['typetree'],'sourceSha256':hashlib.sha256(path.read_bytes()).hexdigest(),'legacy':t['m_Legacy'],'compressed':t['m_Compressed'],'sampleRate':t['m_SampleRate'],'wrapMode':t['m_WrapMode'],'duration':max((k['time'] for c in curves for k in c['keys']),default=0),'curves':curves,'events':t['m_Events'],'pptrCurves':t['m_PPtrCurves'],'compressedRotationCount':len(t['m_CompressedRotationCurves'])}
out={'animations':animations,'clips':list(clips.values()),'validation':{'allReferencesResolved':True,'animationCount':len(animations),'clipCount':len(clips),'curveCount':sum(len(c['curves'])for c in clips.values()),'keyCount':sum(len(curve['keys'])for c in clips.values()for curve in c['curves'])}}
assert len(animations)==6 and len(clips)==2
for clip in clips.values():assert clip['legacy']and not clip['compressed']and not clip['events']and not clip['pptrCurves']and clip['compressedRotationCount']==0
(G/'outgame/task-panel-animations.json').write_text(json.dumps(out,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
print(json.dumps(out['validation']));print([(c['name'],c['duration'])for c in clips.values()])

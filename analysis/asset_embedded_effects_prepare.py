"""Prepare three baseline tower-effect subtrees and exact streamed animation curves.

No new downloads. The established 21-root import remains separate until integration.
"""
import struct,zlib,math
from pathlib import Path
exec((Path(__file__).parent/'asset_skill_native_prepare.py').read_text().split("allowed={'GameObject'")[0])
O=S/'prepared-embedded';O.mkdir(exist_ok=True)
CAB='CAB-68a3634b9a99c5a2a10a36b772e4e945'
root_ids=[CAB+':'+str(i) for i in (-2107951418625338322,832913686255133012,2117273487600732731)]
source_resource=resource
paths={}

def packed_clip(o,row):
 t=tree(o);clip=t['m_MuscleClip']['m_Clip']['data'];assert clip['m_DenseClip']['m_CurveCount']==0 and not clip['m_ConstantClip']['data']
 data=clip['m_StreamedClip']['data'];raw=struct.pack('<'+'I'*len(data),*data);pos=0;frames=[]
 while pos<len(raw):
  time,count=struct.unpack_from('<fI',raw,pos);pos+=8;keys=[]
  for i in range(count):
   index,a,b,c,d=struct.unpack_from('<I4f',raw,pos);pos+=20;keys.append({'index':index,'coefficients':[a,b,c,d]})
  frames.append({'time':time,'keys':keys})
 curves=[]
 for binding in t['m_ClipBindingConstant']['genericBindings']:
  assert not binding['isPPtrCurve'];assert binding['path'] in paths,(o['id'],o['name'],binding,paths);path=paths[binding['path']]
  if binding['typeID']==4:
   assert (binding['attribute'],binding['customType']) in ((3,0),(4,4));props=[('m_LocalScale.' if binding['attribute']==3 else 'localEulerAnglesRaw.')+c for c in 'xyz'];typ='Transform'
  elif binding['typeID']==199:
   assert binding['customType']==22 and binding['attribute']==((zlib.crc32(b'_Alpha')&0x0fffffff)|0x80000000);props=['material._Alpha'];typ='ParticleSystemRenderer'
  else:
   assert binding['typeID']==198 and binding['customType']==27 and binding['attribute']==zlib.crc32(b'looping');props=['looping'];typ='ParticleSystem'
  for prop in props:
   index=len(curves);keys=[]
   for frame in frames:
    if not math.isfinite(frame['time']) or frame['time']<0:continue
    key=next((x for x in frame['keys'] if x['index']==index),None)
    if key:keys.append({'time':frame['time'],'value':key['coefficients'][3],'coefficients':key['coefficients'],'inTangent':0,'outTangent':0,'step':False})
   for a,b in zip(keys,keys[1:]):
    ca,cb,cc,cd=a['coefficients'];dt=b['time']-a['time'];end=((ca*dt+cb)*dt+cc)*dt+cd
    if ca==cb==cc==0 and abs(end-b['value'])>1e-6:a['step']=True
    else:
     assert abs(end-b['value'])<max(2e-5,abs(b['value'])*1e-6),(o['name'],path,prop,end,b['value'])
     a['outTangent']=cc;b['inTangent']=(3*ca*dt+2*cb)*dt+cc
   curves.append({'path':path,'type':typ,'property':prop,'keys':keys,'sourceBinding':binding})
 row.update(curves=curves,duration=t['m_MuscleClip']['m_StopTime'],frameRate=t['m_SampleRate'],legacy=False,loopTime=t['m_MuscleClip'].get('m_LoopTime',False),sourceTypetree=o['outputs']['typetree'],sourceSha256=hashlib.sha256((R/o['outputs']['typetree']).read_bytes()).hexdigest())
 assert len(curves)==clip['m_StreamedClip']['curveCount']

def resource(ident):
 if ident in resources:return resources[ident]
 o=objects[ident]
 if o['type'] not in ('AnimatorController','AnimationClip'):return source_resource(ident)
 row={'id':ident,'index':len(resources),'type':o['type'],'name':o['name'],'source':o['source'],'sourcePathId':str(o['pathId'])};resources[ident]=row
 if o['type']=='AnimationClip':packed_clip(o,row);return row
 t=tree(o);c=t['m_Controller'];tos=dict(t['m_TOS']);assert len(c['m_LayerArray'])==1 and len(c['m_StateMachineArray'])==1 and not t['m_StateMachineBehaviours']
 layer=c['m_LayerArray'][0]['data'];sm=c['m_StateMachineArray'][0]['data'];assert not sm['m_AnyStateTransitionConstantArray']
 row.update(layerName=tos[layer['m_Binding']],layerWeight=layer['m_DefaultWeight'],layerBlendingMode=layer['(int&)m_LayerBlendingMode'],ikPass=layer['m_IKPass'],defaultState=sm['m_DefaultState'],parameters=[],states=[],sourceTypetree=o['outputs']['typetree'])
 for p in c['m_Values']['data']['m_ValueArray']:
  assert p['m_Type']==3
  row['parameters'].append({'name':tos[p['m_ID']],'type':p['m_Type'],'defaultInt':c['m_DefaultValues']['data']['m_IntValues'][p['m_Index']]})
 for entry in sm['m_StateConstantArray']:
  state=entry['data'];bt=state['m_BlendTreeConstantArray'][0]['data']['m_NodeArray'];assert len(bt)==1 and not bt[0]['data']['m_ChildIndices']
  st={'name':tos[state['m_NameID']],'clipIndex':resource(resolve(o,t['m_AnimationClips'][bt[0]['data']['m_ClipID']]))['index'],'speed':state['m_Speed'],'cycleOffset':state['m_CycleOffset'],'ikOnFeet':state['m_IKOnFeet'],'writeDefaults':state['m_WriteDefaultValues'],'mirror':state['m_Mirror'],'transitions':[]}
  for entry in state['m_TransitionConstantArray']:
   x=entry['data'];st['transitions'].append({'destination':x['m_DestinationState'],'duration':x['m_TransitionDuration'],'offset':x['m_TransitionOffset'],'exitTime':x['m_ExitTime'],'hasExitTime':x['m_HasExitTime'],'fixedDuration':x['m_HasFixedDuration'],'interruptionSource':x['m_InterruptionSource'],'orderedInterruption':x['m_OrderedInterruption'],'canTransitionToSelf':x['m_CanTransitionToSelf'],'conditions':[{'parameter':tos[v['data']['m_EventID']],'mode':v['data']['m_ConditionMode'],'threshold':v['data']['m_EventThreshold']} for v in x['m_ConditionConstantArray']]})
  row['states'].append(st)
 return row

def walk(go,path=''):
 o=objects[go];t=tree(o);cs=[resolve(o,x['component']) for x in t['m_Component']];tf=next(x for x in cs if objects[x]['type']=='Transform');tr=tree(objects[tf]);result=[{'id':go,'path':path,'components':cs}]
 for child in tr['m_Children']:
  ct=objects[resolve(objects[tf],child)];cg=resolve(ct,tree(ct)['m_GameObject']);result.extend(walk(cg,(path+'/' if path else '')+objects[cg]['name']))
 return result

for rootid in root_ids:
 nodes=walk(rootid);paths.update({zlib.crc32(p.encode()):p for n in nodes for i in range(len(n['path'].split('/'))) for p in ['/'.join(n['path'].split('/')[i:])]});ids=[i for n in nodes for i in [n['id']]+n['components']];local={i:1000000000001+k for k,i in enumerate(ids)};nodepaths={i:n['path'] for n in nodes for i in [n['id']]+n['components']};root_transform=nodes[0]['components'][0];docs=[];components=[]
 for ident in ids:
  o=objects[ident];rr=reader(o);t=rr.read_typetree()
  assert o['type'] in ('GameObject','Transform','ParticleSystem','ParticleSystemRenderer','Animator','MeshRenderer','MeshFilter','SpriteRenderer'),(ident,o['type'])
  def ptr(value,path,owner=o):
   i=resolve(owner,value)
   if i is None or owner['id']==root_transform and path=='.m_Father':return {'fileID':0}
   if i in local:return {'fileID':local[i]}
   return external(i)
  native=native_tree(rr.serialized_type.node,t,ptr);native['m_ObjectHideFlags']=0
  if o['type']=='GameObject':
   assert native.pop('m_Tag')==0;native.update(m_TagString='Untagged',m_Icon={'fileID':0},m_NavMeshLayer=0,m_StaticEditorFlags=0)
  docs.append(document(rr.class_id,local[ident],o['type'],native));components.append({'id':ident,'fileId':str(local[ident]),'type':o['type'],'path':nodepaths[ident]})
 name=objects[rootid]['name'];dest=O/(name+'.prefab.template');dest.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+''.join(docs),encoding='utf8')
 prefabs.append({'name':name,'originalName':name,'templatePath':dest.relative_to(R).as_posix(),'sourceRoot':rootid,'components':components,'sourceComponentCounts':dict(collections.Counter(x['type'] for x in components)),'rootLocalTRS':tree(objects[root_transform]),'attachment':'Tower objEffectRoot; original parent local position0 rotationIdentity scale1, retained child local TRS'})
out={'prefabs':prefabs,'resources':list(resources.values()),'shaderMap':[],'unknowns':[],'bindingEvidence':{'ParticleSystem.looping':'CRC32 exact serialized field name = 925582877','material._Alpha':'source original material property CRC28 | 0x80000000 = 2333735666','packedCurves':'Original streamed cubic coefficients are converted to Hermite values and derivatives; discontinuous constant segments become step tangents. Every continuous segment endpoint is checked.','reference':'https://github.com/AssetRipper/AssetRipper/blob/master/Source/AssetRipper.Processing/AnimationClips/CustomCurveResolver.cs'}}
(O/'native-import.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8');print(json.dumps({'prefabs':len(prefabs),'resources':dict(collections.Counter(x['type'] for x in resources.values()))}))

from pathlib import Path
import json,struct,hashlib
W=Path(__file__).resolve().parent.parent;O=W/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
h=W/'analysis/recover_outgame_manager_registry.py';n={'__file__':str(h)};exec(h.read_text(encoding='utf8').split('rows=[]')[0],n)
u,b,pairs,ts,ms=[n[k] for k in ('u','b','pairs','ts','ms')]
classes=[]
for index in (4117,4109,4110,4111,4112,4113,4114,4115,4116,4275,4268):
 fields=[]
 for j in range(ts[index][18]):
  name,t,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[index][8]+j));fields.append({'offset':u(u(3823136+4*index)+4*j),'name':ms(name),'typeIndex':t})
 classes.append({'typeIndex':index,'name':ms(ts[index][0]),'fields':fields})
methods=json.loads((O/'method-map.json').read_text(encoding='utf8'));chosen=set(range(31543,31580))|{30442,32611,32612,32693,26213,26214,26217,26220};chosen.update(m["metadata"] for m in methods if m["cls"]=="ExtesionMethod")
resolved=[]
for addr in (4029248,4029236,4029220,3955636,3955664,4012508,4012524,4008864,4029216,4029240,4029228,4029232,4029224,4029244,3955784,3955792,3955804,3955812,3955824):
 v=u(addr);idx=(v&0x1ffffffe)>>1;kind=v>>29
 metadata=idx if kind==3 else struct.unpack_from('<3i',n['mem'],483008+12*idx)[0] if kind==6 else None
 if metadata is not None:
  method=n['md'][metadata];resolved.append({'usageAddress':addr,'kind':kind,'metadata':metadata,'class':ms(ts[method[1]][0]),'method':ms(method[0])});chosen.add(metadata)
rows=[{'metadata':m['metadata'],'class':m['cls'],'method':m['method'],'path':m['path'],'sha256':hashlib.sha256((O/m['path']).read_bytes()).hexdigest()} for m in methods if m['metadata'] in chosen]
output={'status':'source-audited-loading-entity-preload-branches','classes':classes,'methods':rows,'resolvedUsages':resolved,'configFields':{'176':'ConfigMgr.dicEntityModel -> EntityModelConfig.AssetName / PathEnum','312':'ConfigMgr.dic_res -> recovered SceneResources map'},'pathEnumOutputs':[('Scene/A'),('Model/WayLine/A.prefab'),('Model/Entity/A.prefab'),('Model/Hero/A.prefab'),('Data/LevelCfg/A.Json'),('Materials/materials/A.mat'),('Materials/Model/A.mat'),('Animation/Soldier/A.controller'),('Model/Scene/A.prefab'),('Data/AnimationTexture/A.bytes'),('Model/SoldierCommon/A.prefab'),('Model/SoldierAnimationIns/A'),('Textures/skinscene/A.png')], 'source11001':{'AssetName':'GameScene/HD4_CJ_1','PathEnum':8,'path':'Model/Scene/GameScene/HD4_CJ_1.prefab'},'remainingMethods':'Entity and preload source bodies implemented; concrete LevelControl/PlayerControl/loading-page dependency composition and full Main/Player acceptance remain outstanding.'}
# Generic-instantiation pointer table independently matched against the Level/Player/UI
# parent ControlBase<T> contexts at inst557/564/574.
instTable=72592
for typeIndex,instIndex in ((4107,557),(4462,564),(4296,574)):
 parentType=u(200288+4*ts[typeIndex][4]);genericClass=u(parentType)
 assert u(genericClass+4)==u(instTable+4*instIndex)
def instArguments(index):
 inst=u(instTable+4*index);count=u(inst);args=u(inst+4);result=[]
 for j in range(count):
  ptr=u(args+4*j);code=(u(ptr+4)>>16)&255;data=u(ptr)
  result.append({'typeCode':code,'typeName':ms(ts[data][0]) if code in (17,18) else {8:'Int32',2:'Boolean'}.get(code,str(code))})
 return result
output['genericInstantiationTable']=instTable
output['preloadDependencies']=[{'usage':address,'instIndex':index,'arguments':instArguments(index)} for address,index in ((3953440,557),(3953496,564),(3953576,574))]
output['failureDictionaryArguments']=instArguments(308)
output['entityRootType']='UnityEngine.GameObject (typeIndex16505 resolves Type6826)'
output['criticalSourceQuirks']=[
 '31550 increments index after removing destroyed entry, skipping shifted next entry; activeSelf, not activeInHierarchy, controls reuse.',
 '31577 only Adds absent template key. A present null or destroyed entry is not replaced on successful reload.',
 '31577 success writes zero to a local TryGetValue output; shared dictionary retains original count.',
 '31577 failure set_Item receives unchanged count, with no increment instruction before recursive helper call. Default count0 can retry without bound; golden test executes bounded iterations.',
 '31559 cache miss initiates template load but always returns null for that request.',
 '31555 fixed ids are1,10,11,12,13,14,15,101,9033,9034; then stars ShipID52/CampID44/isBoss56 and obstacles EnityID8.',
 'PrepareStart callback increments shared counter, resolves UIControl loading page field72, resets its float44 when nonnull, then recurses; PreLoad uses callback-local index and CanLoadNext gate.'
]
(O/'PREFAB_LOADER_SOURCE_EVIDENCE.json').write_text(json.dumps(output,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print('source evidence',len(rows),'methods',len(resolved),'resolved usages')

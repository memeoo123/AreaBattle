"""Publish only reviewed EffectModule and Base/UI/Fly/Line effect source methods."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def write(p,value):
 text=json.dumps(value,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if p.exists()and b'\r\n'in p.read_bytes()else text).encode())
selected=set(range(26756,26788))|set(range(26796,26817))-{26809}
index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 destination=out/row['path'];data=(stage/row['path']).read_bytes()
 if destination.exists()and b'\r\n'in destination.read_bytes():data=data.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')
 destination.write_bytes(data)
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys();write(out/'method-map.json',index)
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in range(3470,3484):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (4034536,4081828,4081692,4081520,4075204,4052152,4052148,4052160,4105784,4104944,4052144,4068608,4054408,4060784,4075860,3997164,4027840,3990984,4001264,4001248,4001220,4000632,4000816,4000708):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
  row['genericTypes']=[]
  for inst in spec[1:]:
   if inst>=0:
    ptr=u(72592+4*inst);row['genericTypes'].append([ms(ts[u(u(u(ptr+4)+4*j))][0])for j in range(u(ptr))])
 usages.append(row)
generic=json.loads((stage/'effect-module-generics.json').read_text());assert len(generic['methods'])==1 and generic['methods'][0]['metadata']==26809
for row in generic['methods']:
 shutil.copy2(stage/row['path'],out/row['path']);row['sha256']=hashlib.sha256((out/row['path']).read_bytes()).hexdigest()
for name in ('effect-module-generics.json','disassembly/EffectModuleWrapper-12437.txt'):shutil.copy2(stage/name,out/name)
config=Path(__file__).resolve().parent.parent/'UnityProject/Assets/AreaBattle/Resources/Data/EffectConfig.json';original=json.loads(config.read_text());task=next(r for r in original['Datas']if r['id']==1016)
assert task==dict(id=1016,type=0,res='hdzd_eff_bxGlow',duration=5.0)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)],fields=fields,usages=usages,generic=generic,folderUsageTable=dict(address=1688124,usages=[u(1688124+4*i)for i in range(4)]),taskEffect=task,findings=[
 'EffectModule3483 has priority20, initializes a new configuration dictionary and sets initialized before invoking its existing completion delegate. Start/Update are empty. ReadEffectData requires the legacy config resource or modern AppSetting flag, logs source diagnostics, skips duplicate rows retaining first, and marks AlreadySetData even on null list. Clear/Shutdown do not reset initialized or AlreadySetData. Reinitialize replaces configurations but retains the read flag and live-effect dictionary.',
 'Shared generic26809 resolves type0/default UIEffect,1 FlyEffect,2 LineEffect,3 BaseEffect. UI/Fly missing parents use UIEffectRoot under UIModule canvas; Scene uses persistent __WorldEffectRoot. Line reads input parent.position before selecting its own RectTransform root under UIModule.UIRoot, with Canvas overrideSorting=true, order100 and Top layer. Missing config logs then dereferences null; missing line parent throws. No placeholder success is returned.',
 'CreateEffect constructs/starts before optional SetOrder and casts generic T after all creation side effects. Sorting order -9999 disables SetComponent; -1 skips explicit override. Other orders call SetOrder with Top for layer -1, otherwise SortingLayer.IDToName. Direct CreateEffect is unregistered. Show registers UID then assigns completion; completion checks captured UID but removes the argument UID. Close removes the dictionary entry BEFORE virtual Dispose. Clear enumerates live Values and disposes before clearing effects then configs; failures propagate without rollback.',
 'BaseEffect allocates process-shared UID under a lock, sets config/local position/Euler rotation/parent/component flag then virtual Start. Minimal parent-only constructor does not allocate UID or start. Start kicks async-void load, awaits a real WaitUntil(IsLoaded||IsDisposed), then virtual Play. Loading sets IsLoaded false; modern awaits a handle and instantiates, legacy awaits already-instantiated prefab. It parents with worldPositionStays=false, applies local position/Euler angles, calls SetComponent, then marks loaded. Prefab local scale is retained.',
 'Resource paths are Effect/ plus UI/,Fly/,Line/,Scene/ for types0..3 respectively; unknown type folder is String.Empty. Modern asset acquisition remains a required explicit service, using existing real OutgameAssetHandle for Instantiate and Release. Original1016 is UI/hdzd_eff_bxGlow with5 seconds. No original1016 visual prefab or particle equivalence is claimed by this milestone.',
 'Source load has no disposed/cancellation/null-result guard. Early Close satisfies Await; base Play can Stop on missing object and complete, while later load still instantiates and parents a now-unregistered orphan. This source race is preserved and directly tested; fixture cleanup explicitly disposes the late orphan. It is not advertised as fixed or leak-free.',
 'SetActive writes native active state if present then its flag. SetOrder stores layer/order first; when native object exists it writes absolute order to active child Renderers and ADDS order to active child Canvases. No inactive inclusion or overrideSorting is added. Missing native object retains pending order. SetComponent gates on component flag/object, inherits parent Canvas layer/order+1 when order=-1, otherwise only applies pending order.',
 'Base Play stops/completes on absent native object; otherwise applies requested active state and awaits scaled WaitForSeconds for positive double duration cast tofloat. After timer it returns if disposed; otherwise Stop then completion. Zero/negative/NaN duration has no timer. Dispose sets disposed first, Destroy if native exists, then handle.Release without clearing fields or invoking completion. Destroy failure prevents release; repeated calls preserve handle warning semantics.',
 'Fly Play sets active, rotates from Vector3.up toward target-current world position, uses DOMove(target,flyTime,false) and completes by virtual Dispose then completion. It does not use config duration or cancel tween on early Dispose. Required native Move endpoint uses existing recovered tween runner with source default ease/scaled timing in PlayMode validation.',
 'Line helper26782 invokes BaseEffect.Play (table24616), not SetComponent. Thus Line Play starts the base duration timer, sets direction target-origin, projects both points using UIModule camera and backing LineEffectRoot, scales Y to projected distance/256 retaining X/Z, assigns world origin and active state, then starts a SECOND scaled duration timer. Its own timer has no disposed guard and calls Stop plus completion. Repeated Dispose/Release diagnostics are source behavior, verified under actual asset handle ownership.',
 'WorldToRectLocalPoint uses backing line-root field without creating it and ignores ScreenPointToLocalPointInRectangle boolean. Module root shutdown follows child disposal then world/UI/line roots in order, nulling only after successful native destruction.',
 'EffectCellection4221 Show fourth argument is integer sorting-layer ID0, not boolean. Interface and both previous fixture implementations are corrected while preserving original call value, order and cache semantics. The existing collection now composes with actual recovered EffectModule in validation.',
 'Full EffectControl4058 pooling/sequence/fly controller lifecycle, general resource/config acquisition composition, original particle assets/task page1016 binding, production Main/account/platform/all remaining business and Player/original audiovisual acceptance are still pending. Controller count remains20/38.'
])
write(out/'EFFECT_MODULE_SOURCE_EVIDENCE.json',report)
print(json.dumps(dict(methods=len(selected),fields=len(fields),usages=len(usages),genericMethods=1,indexedMethods=len(index))))

"""Publish only reviewed TaskMgr/TaskOffStrategy and shared generic source evidence."""
from pathlib import Path
import argparse,hashlib,json,shutil,struct
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';stage=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
def copy_reviewed(name):
 data=(stage/name).read_bytes()
 if name.endswith('.txt'):
  destination=out/name
  ending=b'\r\n'if destination.exists()and b'\r\n'in destination.read_bytes()else b'\n'
  data=ending.join(line.rstrip()for line in data.splitlines())+ending
 (out/name).write_bytes(data)
selected=set(range(35494,35512));index=json.loads((out/'method-map.json').read_text());known={r['metadata']:r for r in index}
for row in json.loads((stage/'method-map.json').read_text()):
 if row['metadata'] not in selected:continue
 copy_reviewed(row['path'])
 if row['metadata'] not in known:index.append(row);known[row['metadata']]=row
 else:assert row==known[row['metadata']]
assert selected<=known.keys()
p=out/'method-map.json';text=json.dumps(index,ensure_ascii=False,indent=2)+'\n';p.write_bytes((text.replace('\n','\r\n')if b'\r\n'in p.read_bytes()else text).encode())
methods=[dict(known[i],sha256=hashlib.sha256((out/known[i]['path']).read_bytes()).hexdigest())for i in sorted(selected)]
generic=json.loads((stage/'task-manager-generics.json').read_text());assert len(generic['methods'])==5
paths=['task-manager-generics.json']+[r['path']for r in generic['methods']]+['disassembly/TaskManagerWrapper-'+str(i)+'.txt'for i in (11278,11277,2575)]
for name in paths:copy_reviewed(name)
genericFiles=[dict(path=name,sha256=hashlib.sha256((out/name).read_bytes()).hexdigest())for name in paths]
b,mem,ts,md,ms,u,pairs=[c[k]for k in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4566,4692,4694,4695):
 for k in range(ts[owner][18]):
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=u(u(3823136+4*owner)+4*k),typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3981476,3981480,3981484,3981488,3988868,4016596,4030848,4030852,4004860,4105372,4105256):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=idx)
 if kind==5:
  length,offset=struct.unpack_from('<II',b,pairs[0][0]+8*idx);row['literal']=b[pairs[1][0]+offset:pairs[1][0]+offset+length].decode()
 elif kind==3:row.update(sourceClass=ms(ts[md[idx][1]][0]),method=ms(md[idx][0]))
 elif kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);row.update(spec=list(spec),sourceClass=ms(ts[md[spec[0]][1]][0]),method=ms(md[spec[0]][0]))
 usages.append(row)
ptr=u(72592+3387*4);assert u(ptr)==3;argv=u(ptr+4);types=[u(u(argv+4*k))for k in range(3)];assert types==[4686,4685,4692]
contexts=[]
for i in range(24):
 token,start,count=struct.unpack_from('<3I',mem,2207936+12*i)
 if token!=ts[4566][-1]:continue
 for slot in range(count):
  kind,address=struct.unpack_from('<2I',mem,2208224+8*(start+slot));idx=u(address);row=dict(slot=slot,kind=kind,address=address,index=idx)
  if kind==3:
   spec=struct.unpack_from('<3i',mem,483008+12*idx);method=md[spec[0]];row.update(spec=list(spec),method=ms(ts[method[1]][0])+'.'+ms(method[0]))
  contexts.append(row)
assert len(contexts)==7
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,genericFiles=genericFiles,concreteGenericInstantiation=dict(index=3387,types=types),runtimeGenericContext=contexts,
 findings=[
  'TaskMgr4692 OnInit constructs TaskOffStrategy only; InitStrategy captures strategy and binds callback/activity1300001 through NetStratrgyBase.InitData. Generic init stores activityID/callback, resolves TaskMgr from DataManagerPool, then checks global Activities config before virtual slot5 requests UpdateData(true). Wrapper11277 supplies literal true to shared34692. Server wait/local selection remains existing DataManagerBase behavior.',
  'Manager callback publishes input Data, clears dictionary, Add indexes every live input child; duplicate key failure preserves input and indexed prefix, skips activity lookup. Current ActivityControl TaskActivity1300001 receives captured input after complete indexing. Release invokes empty strategy slot6 and retains references; save forwards slot7.',
  'TaskOffStrategy load catches decompression/first JSON exceptions and retries raw JSON; second parse error propagates. Fresh data enumerates TaskActivities.Values and publishes each new child before ResetExtraRefreshTimes. Fresh branch skips existing-data migration. Both branches then enumerate TaskGroupsByActivity, append missing activity children only after successful reset, and invoke nullable callback.',
  'Existing data corrects ext.activityID, removes children absent from TaskGroupsByActivity, and when extraRefreshNum!=-1 removes unknown groups, resets remaining group extra counters, then sets sentinel-1. Sentinel-1 bypasses even invalid group validation. Missing tasks are diagnosed before removal, with adjacent removed entries revisited.',
  'Nonzero old Int64 progress appends only first configured condition, copied middle args and old value, then zeros old progress. Zero progress path enumerates current conditions, appends only keys/args missing via Condition.KeyEqual, and logs task id/description/new key after append. Existing unmatched records and their values are retained. UID zero becomes sign-extended object.GetHashCode only after migration succeeds; existing UID survives.',
  'Save serializes current strategy Data to JSON, deserializes typed independent TaskData, serializes again and compresses before resolving cached activity and calling TaskActivity.SaveData; then current manager.SaveLocalData writes captured text. Activity mutation is deferred to later save. Null cached activity retries lookup; missing owner throws, activity failure prevents persistence. Callback lookup in manager is current, independently of strategy cached save owner.',
  'Generic registration provides five canonical shared bodies; concrete wrappers11278/11277/2575 and method usages resolve instantiation3387 TaskData/TaskActivity/TaskMgr. Seven RGCTX slots identify ActivityControl.GetActivity, DataManagerPool.GetModel and virtual initialization. Generic field offsets listed0 are metadata placeholders, not concrete runtime offsets.',
  'Validation uses actual configuration/pool/account-aware storage and independent file restart. Concrete TaskActivity callbacks are explicit test endpoints. TaskMgr is not yet installed into complete production Activity runtime; child task strategy/daily refresh/claim/UI and Main remain pending.'
 ])
(out/'TASK_MANAGER_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),genericMethods=len(generic['methods']),contexts=len(contexts),indexedMethods=len(index))))

"""Publish scoped ObjectPoolManager original normal/shared bodies and runtime generic contexts."""
from pathlib import Path
import argparse,hashlib,json,struct,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);stage=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=stage/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
j=json.loads((source/'pool-manager-generics.json').read_text());assert {r['metadata']for r in j['methods']}=={27496,28718,28719,28720,28722}
for r in j['methods']:
 shutil.copy2(source/r['path'],out/r['path']);r['sha256']=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()
(out/'pool-manager-generics.json').write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n')
index=json.loads((out/'method-map.json').read_text());selected={28712,28713,28714,28715,28716,28717,28721,28723,27497,28679,28682}
methods=[dict(r,sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest())for r in index if r['metadata']in selected];assert len(methods)==11
contexts=[]
for methodId in [28718,28719,28720,28722]:
 token=c['md'][methodId][6]
 for k in range(112):
  tok,start,count=struct.unpack_from('<3I',c['mem'],2249520+12*k)
  if tok!=token:continue
  rows=[]
  for i in range(count):
   kind,address=struct.unpack_from('<II',c['mem'],2250864+8*(start+i));data=c['u'](address);row=dict(slot=i,kind=kind,address=address,index=data)
   if kind==3:
    spec=struct.unpack_from('<3i',c['mem'],483008+12*data);m=c['md'][spec[0]];row.update(spec=list(spec),sourceClass=c['ms'](c['ts'][m[1]][0]),method=c['ms'](m[0]))
   rows.append(row)
  contexts.append(dict(metadata=methodId,token=hex(token),rangeIndex=k,start=start,count=count,rows=rows))
assert len(contexts)==4
usages=[]
for address in [4000612,4002684,4044088,4101756]:
 encoded=c['u'](address);kind=encoded>>29;ix=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind,index=ix)
 if kind==6:row['spec']=list(struct.unpack_from('<3i',c['mem'],483008+12*ix))
 elif kind==5:
  length,offset=struct.unpack_from('<II',c['b'],c['pairs'][0][0]+8*ix);row['literal']=c['b'][c['pairs'][1][0]+offset:c['pairs'][1][0]+offset+length].decode('utf8')
 usages.append(row)
report=dict(status='original-pool-manager-frame-lifecycle-and-generic-factory-recovered',metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,generic=j,contexts=contexts,usages=usages,prefabObjectType=dict(typeIndex=3665,name=c['ms'](c['ts'][3665][0]),namespace=c['ms'](c['ts'][3665][1])),typeFullNameGetter=dict(metadata=2774,typeIndex=288,slot=26),findings=[
 'ObjectPoolManager3672 has constructor-owned Dictionary<string,ObjectPoolBase> field16. Initialize28712 sets initialized flag before invoking retained callback; does not clear/recreate dictionary. Priority70, Start empty. Update28716 iterates live dictionary values and virtual slot11 with both unchanged deltas, without null guard. Mutation and exceptions abort iteration as original.',
 'Shutdown28717 iterates live entries, skips managed-null values, calls slot12 on each nonnull pool, then clears dictionary and resets initialized=false. Failure leaves all entries and initialized state; retry may invoke earlier pools again. Initialized delegate retained.',
 'Normal generic28718/28720 route through Text.GetFullName27496/27497: runtime Type.FullName getter slot26/2774, null or empty pool name returns type full name, otherwise original format {0}.{1}. NormalPool object3665 is original globally named g\\u0093t\\u0090\\u00b6O, not a renamed restoration CLR type. Dot collisions and null/empty alias are preserved. Generic factory API accepts source type full name plus concrete original-pool specialization factory; other object families remain caller-owned.',
 'CreateSingleSpawnObjectPool28719 calls InternalCreate with allowMultiSpawn=false, capacity Int32.MaxValue, expireTime Single.MaxValue, priority0. InternalCreate28722 checks typed key before constructing pool; duplicate emits original Already exist object pool message and framework exception. Successful construction precedes key recomputation and Dictionary.Add; reentrant duplicate Add fails retaining the previously inserted owner.',
 'InternalDestroy28723 TryGetValue miss returnsfalse. Hit calls pool.Shutdown before Dictionary.Remove and returns actual Remove boolean. Release failure retains registration; callback removal makes final returnfalse; unlike full manager shutdown, null entry is not skipped.',
 'OutgameObjectPool now implements managed lifecycle contract; existing original register/spawn/unspawn/capacity/DateTime expiry/real-delta update/shutdown logic reused unchanged. NormalPool creation selects the actual manager single-spawn factory and applies original300/5/5/0 settings.',
 'NormalPool28679 and28682 BOTH resolve GameFrameEntry.GetModule26446 at usage4000612. They do not use HaveModule. OutgameNormalPoolLifetime/Bind therefore acquires through actual Frame.GetModule even after a cleared frame, preserving new uninitialized manager creation followed by no-op destroy. Root/handle/resource teardown retains original order; resource unloading remains required endpoint.',
 'Actual FrameEntry priority sorting places manager70 before Logic12 in update and reverses them at shutdown. EffectControl disposal removes its registered pool and handles resource hooks while manager is still present; then manager shutdown clears leftover pools and flag. Native validation drives Frame.Update from actual UpdateManager, including paused scaled time and real pool expiration.',
 'Source framework exception is exposed as restoration-local OutgameFrameworkException. Existing OutgameObjectPool argument validation still uses earlier InvalidOperationException projection. General non-prefab pool families require their actual recovered object implementations/factories; no invented success factory supplied.',
 'Full production Main/resource catalog/TopInfo account/SDK/platform/all remaining17 controllers and business/final Player/original audiovisual equivalence remain pending. Pool manager acquisition fixture is removed in this composition; native resource acquisition/unload, audio and external report/platform remain explicitly local test hosts.'
])
(out/'POOL_MANAGER_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(normalMethods=len(methods),genericMethods=len(j['methods']),contexts=len(contexts),usages=len(usages),indexedMethods=len(index))))

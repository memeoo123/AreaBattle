"""Retain original activity controller bodies, shared generics and field/usage evidence."""
from pathlib import Path
import argparse,hashlib,json,struct,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
selected=set(range(35207,35250))-{35231,35236};selected.update([35198,35268])
index=json.loads((out/'method-map.json').read_text());known={r['metadata'] for r in index};added=[]
def write(path,value):
    raw=path.read_bytes() if path.exists() else b'';text=json.dumps(value,ensure_ascii=False,indent=2)+('\n' if not raw or raw.endswith(b'\n') else '')
    path.write_bytes((text.replace('\n','\r\n') if b'\r\n' in raw else text).encode())
for row in json.loads((source/'method-map.json').read_text()):
    if row['metadata'] in selected and row['metadata'] not in known:
        shutil.copy2(source/row['path'],out/row['path']);index.append(row);known.add(row['metadata']);added.append(row['metadata'])
if added:write(out/'method-map.json',index)
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected]
assert len(methods)==43
generic=json.loads((out/'activity-owner-generics.json').read_text());shared=[]
for row in generic['methods']:
    if row['metadata'] not in [35231,35236]:continue
    row['implemented']=True;assert row['sha256']==hashlib.sha256((out/row['path']).read_bytes()).hexdigest();shared.append(row)
write(out/'activity-owner-generics.json',generic)
fields=[]
for owner in [4637,4638,4644,4645,3247,3248,3256]:
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
        fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
usages=[]
for address in [3955116,3955144,3955160,3955048,3994500,4011364,4011368,4011340]:
    encoded=c['u'](address);assert encoded>>29==6;spec=struct.unpack_from('<3i',c['mem'],483008+12*((encoded&0x1ffffffe)>>1));method=c['md'][spec[0]]
    usages.append(dict(address=address,spec=list(spec),sourceClass=c['ms'](c['ts'][method[1]][0]),method=c['ms'](method[0])))
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,sharedGenerics=shared,fields=fields,methodUsages=usages,
    findings=[
        'All36 ActivityControl4637 methods:34 normal bodies plus two shared generic GetActivity overloads. Nested button comparator35245 ascending signed btnPriority; popup comparators35246/47 descending signed popPriority, no subtraction overflow. Empty ActivityInitData4644 and registration attribute4638 constructor/getter verified.',
        'Init35226 stores/defaults InitData before Cleanup, removes then appends the same global item hook, appends frame Dispose, creates fresh ActivityManager and AddModel4658(autoSyn=true), then reads config. ConfigReady35212 adds listeners before reflection, clears item index, snapshots root Values, initializes offline manager. Pool duplicate retains earlier manager while controller proceeds with newly allocated one.',
        'Register35237 scans base subclasses bearing ActivityControlRegister twice. First instantiates configured roots with Add; second generic fallback fills root-only rows. Third instantiates attributed children, overwrites child map and appends the same child to each existing parent (duplicates allowed), sets parent child dictionary; unregistered child rows have no fallback. Both reflection passes diagnose missing ids. Pending roots overwrite after child attachment then pending map clears.',
        'DataReady35228 registers daily0/86400 using last timestamp once, then overwrites item index for every saved row before refreshing any root. Deferred widget tuples deduplicate each Button/Text separately (null allowed); deferred UI tuple sets types. Type.Equals(ActivityBase), virtual slot126/metadata2883 (not IsSubclassOf slot21), gates an additional direct child Refresh loop only for exact base roots; base Refresh already recursively refreshed children. Derived roots do not run the extra loop. Arrangement groups all present roots/children with Data by config.areaID, sorts config.btnPriority, reorders actual Button sibling index and replaces listeners.',
        'Item handler35239 enumerates live root Values, virtual slot4 ActivityFactoryBase(item.id) then interface Produce; first nonnull entity returns, otherwise each immediate child is tried before next root. Null factory fails and no catch fallback is added. Existing base factory Produce null behavior is preserved.',
        'Notice/Launch queues retain duplicates, sort descending. Automatic notice35233 skips noticeAutoPop!=1, marks data.noticePop then current singleton dirty before virtual UI open; removes only one matched queue index after successful return. OpenUI marks both queues by exact Type equality. CloseUI uses forward Remove(item) so adjacent matches are skipped; automatic notice already removed from queue can leave NoticeUi reference blocking later automatic pops.',
        'Update35238 runs live root BaseUpdate, automatic popup, accumulates unscaled delta and subtracts60 once, then refreshes statistics. Dirty save is gated by pool IsEnableSaveData and clears dirty only after successful Manager.OnSave. Daily35223 writes last timestamp first, skips null/zero-launch records, computes unchecked signed TimeSpan ticks/days regardless state, does not mark activity dirty.',
        'Minute statistics35211 counts notice elapsed10600 and prelaunch date remainder10700 with signed truncating /60000 and live saved row reads after GameValue provider. Daily likewise rereads row launch/id after provider. No clamp or snapshot replaces original callback-sensitive ownership.',
        'Launch35225/Warm35242 call virtual activity hook first, capture Data for timestamp write across clock provider, reread Data to clear opposite stamp. Launch/Over reset10600 then10700. Report host creates original kind with(true,null); activity localized name then parent report string assigned, CommonModule_Activity_Lunch (source typo) / CommonModule_Activity_WarmOnceOnLT sent before resolving report host again for delivery. Over clears both stamps on same Data and has no common event. Report extra fields/serialization/delivery stay host-owned.',
        'GetParentReportStr35198 caches nonempty parentName; otherwise resolves each parent through current config owner, localizes name, appends underscore by original array index, so missing final parent preserves trailing underscore. Empty output recomputes on next call. Cleanup35230 releases root list first then item/root/child maps, clears registration callback, removes listeners/daily, disposes config and removes frame delegate. It retains pending roots, popup queues/UI, widgets, manager/list/time and item hook. RemoveListeners35235 restores daily registration flag; Dispose clears global singleton even for stale receiver.'
    ],boundaries=[
        'This restores common activity controller behavior and concrete generic runtime composition, not attributed concrete gameplay classes/managers or their UI/reward business logic. It is independent of38 LogicModule controllers; lifecycle count remains18/38.',
        'Runtime binds actual recovered config/offline storage/statistics/FSM/Unity UpdateManager/daily scheduler/frame-disposal owners; app must supply concrete activity/manager factories, UI/report delivery, item service/account/platform dependencies. No fabricated network success or reward entity.',
        'Integrated/native validation uses explicit report/UI endpoints and deterministic/captured scheduler registration with actual source control logic. Native checks establish frame/pointer/save/restart behavior; no original page audiovisual acceptance or Player build claim.'
    ])
write(out/'ACTIVITY_CONTROL_SOURCE_EVIDENCE.json',report)
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),sharedGenerics=len(shared),fields=len(fields))))
